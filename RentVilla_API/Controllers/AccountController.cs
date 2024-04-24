using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RentVilla_API.Data;
using RentVilla_API.DTOs;
using RentVilla_API.Entities;
using RentVilla_API.Interfaces;
using RentVilla_API.Logger.LoogerInterfaces;
using Serilog;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace RentVilla_API.Controllers
{
    public class AccountController : BaseAPIController
    {
        private readonly ITokenService _tokenService;
        private readonly ILoggerDev _loggerDev;
        private readonly IUserRepository _userRepository;
        private readonly AppDBContext _appDBContext;
        protected APIResponse _response;
        private readonly IMapper _mapper;
        private readonly List<string> _notAllowedProperties = new List<string> { "username", "email", "passwordhash", "passwordsalt", "created" };

        public AccountController(ITokenService tokenService, AppDBContext appDBContext, ILoggerDev loggerDev,
            IUserRepository userRepository, IMapper mapper)
        {
            _tokenService = tokenService;
            _appDBContext = appDBContext;
            _loggerDev = loggerDev;
            _userRepository = userRepository;
            _response = new();
            _mapper = mapper;
        }

        
        [HttpPost("register")] // api/account/register

        public async Task<ActionResult<APIResponse>> Register(RegisterDTO registerDTO)
        {
            bool checkUnique = await IsUniqueUser(registerDTO);

            if (!checkUnique)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Email is already exist!!");
                _response.Result = registerDTO.Email;
                _loggerDev.Log("Email already exist", "error");
                return BadRequest(_response); ;
            }

            if (registerDTO.Email == null || registerDTO.Password == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Registration Error");
                _loggerDev.Log("Password or Email fields or both is Empty!!", "error");
                return BadRequest(_response);
            }

            var userPassCheck = IsPasswordCorrect(registerDTO.Password);

            if (!userPassCheck)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Invalid password format");
                _response.Result = registerDTO.Password;
                _loggerDev.Log("Invalid password format", "error");
                return BadRequest(_response);
            }

            var userEmailCheck = IsEmailValid(registerDTO.Email);
            if (!userEmailCheck)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Invalid Email format");
                _response.Result = registerDTO.Email;
                _loggerDev.Log("Invalid EMAIL format", "error");
                return BadRequest(_response);
            }

            _loggerDev.Log("Email and Password accepted", "info");

            using var hmac = new HMACSHA512();

            _loggerDev.Log("Generating Token for new user", "info");


            var appUser = new AppUser
            {
                Email = registerDTO.Email.ToLower(),
                UserName = await GetuserNameFromEmail(registerDTO),
                PasswordHash = ComputeHashValue(hmac, registerDTO.Password),
                PasswordSalt = hmac.Key,
                Created = DateTime.Now,
            };

            _loggerDev.Log("Username extracted from " + appUser.Email + " and value is:: " + appUser.UserName, "info");

            _loggerDev.Log("Token generated", "info");

            _appDBContext.Users.Add(appUser);
            _loggerDev.Log("User added to DataBase", "info");

            try
            {
                var userDTO = new UserDTO
                {
                    Email = appUser.Email,
                    Token = _tokenService.CreateToken(appUser),
                };
                _loggerDev.Log("Registration Token is generated", "info");

                await _appDBContext.SaveChangesAsync();

                _loggerDev.Log("Changes saved in DataBase", "info");
                _response.StatusCode = HttpStatusCode.Created;
                _response.ResponseIsSuccessfull = true;
                _response.ErrorMessages = null;
                _response.Result = _mapper.Map<UserDTO>(userDTO);
                return this.Created(string.Empty, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Exception thrown to Logs!");
                _loggerDev.Log(ex.Message.ToString(), "error");
                return BadRequest(_response);
            }

        }

        [HttpPatch("changeUserPass")]

        public async Task<ActionResult<APIResponse>> ChangeUserPassword(ChangePasswordDTO changePasswordDTO, int id)
        {
            var user = await _appDBContext.Users.SingleOrDefaultAsync(user => user.Id == id);

            if (user == null || id==0) 
            {
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("User not found");
                _response.Result = null;
                _loggerDev.Log("User not found", "error");
                return Unauthorized(_response);
            }

            using var hmac = new HMACSHA512(user.PasswordSalt);
            _loggerDev.Log("Get User OLD Password Salt from DataBase", "info");


            var computedHash = ComputeHashValue(hmac, changePasswordDTO.Password);
            _loggerDev.Log("Calculate HashValue of OLD Password", "info");

            for (int i = 0; computedHash.Length > i; i++)
            {
                _loggerDev.Log("Start to compare calculated hash value with stored hash value for: " + i, "info");
                if (computedHash[i] != user.PasswordHash[i])
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ResponseIsSuccessfull = false;
                    _response.ErrorMessages.Add("Invalid OLD Password");
                    _response.Result = null;
                    _loggerDev.Log("Password hash values are not equal for OLD password!", "error");
                    return BadRequest(_response);
                }
            }

            var userNewPassCheck = IsPasswordCorrect(changePasswordDTO.NewPassword);
            _loggerDev.Log("Start to check NEW Password format!", "warning");

            if (!userNewPassCheck)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Invalid NEW password format");
                _response.Result = changePasswordDTO.NewPassword;
                _loggerDev.Log("Invalid NEW password format", "error");
                return BadRequest(_response);
            }

            _loggerDev.Log("NEW Password format is correct", "info");

            using var hmacNewPass = new HMACSHA512();
            _loggerDev.Log("New hmac created for NEW Password", "info");

            try 
            {
                user.PasswordSalt = hmacNewPass.Key;
                user.PasswordHash = ComputeHashValue(hmacNewPass, changePasswordDTO.NewPassword);
                _loggerDev.Log($"New Password Salt and Hash Values are Generated for {user.UserName}", "info");

                var userDTO = new UserDTO
                {
                    Email = user.Email,
                    Token = _tokenService.CreateToken(user)
                };
                _loggerDev.Log("New Password Token is generated", "info");
                await _appDBContext.SaveChangesAsync();
                _loggerDev.Log("Changes are saved in DataBase", "info");

                _response.StatusCode = HttpStatusCode.OK;
                _response.ResponseIsSuccessfull = true;
                _response.ErrorMessages = null;
                _response.Result = changePasswordDTO;
                _loggerDev.Log("Password change is Successfull!", "info");
                return Ok(_response);
            }
            
            catch (Exception ex) 
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Exception thrown to Logs!");
                _loggerDev.Log(ex.Message.ToString(), "error");
                return BadRequest(_response);
            }
            
        }

        [HttpPost("login")]
        public async Task<ActionResult<APIResponse>> Login(LoginDTO loginDTO)
        {
            var user = await _appDBContext.Users.SingleOrDefaultAsync(
               option => option.Email == loginDTO.Email);
            _loggerDev.Log("Get Email from DataBase to Login", "info");

            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Invalid Email");
                _response.Result = _mapper.Map<LoginDTO>(loginDTO);
                _loggerDev.Log("Invalid Email", "error");
                return Unauthorized(_response);
            }

            using var hmac = new HMACSHA512(user.PasswordSalt);
            _loggerDev.Log("Get User Password Salt from DataBase to Login", "info");

            var computedHash = ComputeHashValue(hmac, loginDTO.Password);
            _loggerDev.Log("Calculate HashValue to Login", "info");

            for (int i = 0; computedHash.Length > i; i++)
            {
                _loggerDev.Log("Start to compare calculated hash value with stored hash value for: " + i, "info");
                if (computedHash[i] != user.PasswordHash[i])
                {
                    _response.StatusCode = HttpStatusCode.Unauthorized;
                    _response.ResponseIsSuccessfull = false;
                    _response.ErrorMessages.Add("Invalid Password");
                    _response.Result = _mapper.Map<LoginDTO>(loginDTO);
                    _loggerDev.Log("Login Failed: Password hash values are not equal!", "error");
                    return Unauthorized(_response);
                }
            }

            _loggerDev.Log("Password hash values are equal!", "info");

            try
            {
                var userDTO = new UserDTO
                {
                    Email = user.Email,
                    Token = _tokenService.CreateToken(user)
                };
                _loggerDev.Log("Login Token is generated", "info");
                _response.StatusCode = HttpStatusCode.OK;
                _response.ResponseIsSuccessfull = true;
                _response.ErrorMessages = null;
                _response.Result = _mapper.Map<UserDTO>(userDTO);
                _loggerDev.Log("Login is Successfull!", "info");
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Exception thrown to Logs!");
                _loggerDev.Log(ex.Message.ToString(), "error");
                return BadRequest(_response);
            }

        }


        [HttpPost("UserAddress")]

        public async Task<ActionResult<APIResponse>> CreateUserAddress(AppUserAddressDTO appUserAddress, int id)
        {
           var userId =  await _userRepository.GetUserByIdAsync(id);

            if (userId == null || id==0) 
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("User does NOT Exist");
                _response.Result = null;
                _loggerDev.Log($"Create User Address: User with id = {id} is NOT exist!!", "error");
                return NotFound(_response);
            }

            var phoneNumCheck = IsPhoneNumberValid(appUserAddress.PhoneNumber);
            
            if(!phoneNumCheck)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add($"{appUserAddress.PhoneNumber} is incorrect format");
                _response.Result = null;
                _loggerDev.Log($"Create AppUserAddress: {appUserAddress.PhoneNumber} is incorrect format","error");
                return BadRequest(_response);
            }

            try 
            {
                var appUserAddressCreate = new AppUserAddress
                {
                    AppuserID = userId.Id,
                    AppUser = userId,
                    Email = userId.Email,
                    PhoneNumber = appUserAddress.PhoneNumber,
                    AddressLine1 = appUserAddress.AddressLine1,
                    AddressLine2 = appUserAddress.AddressLine2,
                    ZipCode = appUserAddress.ZipCode,
                    State = appUserAddress.State,
                    City = appUserAddress.City,
                    Created = DateTime.Now
                };

                await _appDBContext.AddAsync(appUserAddressCreate);
                await _appDBContext.SaveChangesAsync();
                _response.StatusCode = HttpStatusCode.OK;
                _response.ResponseIsSuccessfull=true;
                _response.ErrorMessages = null;
                _response.Result = appUserAddress;

                return Ok(_response);
            }
            catch (Exception ex) 
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Exception thrown to Logs!");
                _loggerDev.Log(ex.Message.ToString(), "error");
                return BadRequest(_response);
            }
            
        }

        //Will use for future referances - DO NOT DELETE!
        //[HttpPatch("editUser")]
        //public async Task<ActionResult<APIResponse>> EditWithAdo(int id, JsonPatchDocument<AppUserDTO> appUserDTO)
        //{

        //    /*
        //     * "path": "username",
        //    "op": "replace",
        //    "value": "test"
        //     */

        //    if (id == 0)
        //    {
        //        _response.StatusCode = HttpStatusCode.NotFound;
        //        _response.ResponseIsSuccessfull = false;
        //        _response.ErrorMessages.Add($"User does NOT exist!");
        //        _response.Result = null;
        //        return NotFound(_response);
        //    }

        //    var conStr = await Task.Run(() => (SqlConnection)_appDBContext.Database.GetDbConnection());
        //    // Check if the update operation is targeting an allowed column
        //    if (_notAllowedProperties.Contains(appUserDTO.Operations[0].path))
        //    {
        //        _loggerDev.Log("Updating this data is not allowed.", "error");
        //        _response.StatusCode = HttpStatusCode.BadRequest;
        //        _response.ResponseIsSuccessfull = false;
        //        _response.ErrorMessages.Add($"Updating {appUserDTO.Operations[0].path} data is not allowed.");
        //        _response.Result = null;
        //        return BadRequest(_response);
        //    }

        //    SqlCommand cmd = new SqlCommand("update users set " + appUserDTO.Operations[0].path + "='" + appUserDTO.Operations[0].value + "'where Id=" + id + "", conStr);
        //    _loggerDev.Log($"VALUE:: {appUserDTO.Operations[0].path} CAHANGED WITH:: {appUserDTO.Operations[0].value}", "warning");

        //    if (appUserDTO.Operations[0].value == null)
        //    {
        //        _response.StatusCode = HttpStatusCode.NotFound;
        //        _response.ResponseIsSuccessfull = false;
        //        _response.ErrorMessages.Add($"User does NOT exist!");
        //        _response.Result = null;
        //        return NotFound(_response);
        //    }

        //    try
        //    {

        //        conStr.Open();
        //        int x = await cmd.ExecuteNonQueryAsync();
        //        conStr.Close();
        //        if (x > 0)
        //        {
        //            _loggerDev.Log("Changes saved in DataBase", "info");
        //            _response.StatusCode = HttpStatusCode.OK;
        //            _response.ResponseIsSuccessfull = true;
        //            _response.ErrorMessages = null;
        //            _response.Result = appUserDTO;
        //            return Ok(_response);
        //        }
        //        else
        //        {
        //            return BadRequest(_response);
        //        }

        //    }
        //    catch (Exception ef)
        //    {
        //        _response.StatusCode = HttpStatusCode.BadRequest;
        //        _response.ResponseIsSuccessfull = false;
        //        _response.ErrorMessages.Add("Exception thrown to Logs!");
        //        return BadRequest(ef.Message);
        //    }

        //}

        //PARTIAL UPDATE!
        [HttpPut("EditUserAdress")]
        public async Task<ActionResult<APIResponse>> EditUserAddress(int id, AppUserAddressDTO updatedUserAddress)
        {

            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Invalid User");
                _loggerDev.Log($"Edit User Address: -- {id} -- is NOT match or 0", "error");
                _response.Result = null;
                return BadRequest(_response);
            }

            var userAddress = await _appDBContext.UsersAddress.FirstOrDefaultAsync(a => a.AppuserID == user.Id);

            if (userAddress == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Invalid User");
                _loggerDev.Log($"Edit User Address: -- {id} -- is NOT match or 0", "error");
                _response.Result = null;
                return BadRequest(_response);
            }

            if (updatedUserAddress.AppuserID != id) 
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Invalid User");
                _loggerDev.Log($"Edit User Address: -- {id} -- is NOT matching with {updatedUserAddress.AppuserID}", "error");
                _response.Result = null;
                return BadRequest(_response);
            }

            var modelDTO = _mapper.Map(updatedUserAddress, userAddress);
            await _userRepository.UpdateUserAddressAsync(modelDTO);

            _response.StatusCode = HttpStatusCode.OK;
            _response.ResponseIsSuccessfull = true;
            _response.ErrorMessages=null;
            _loggerDev.Log($"Edit User Address: -- {id} -- is NOT match or 0", "error");
            _response.Result = updatedUserAddress;
            return Ok(_response);
        }

        [HttpDelete("deleteUser")]
        public async Task<ActionResult<APIResponse>> DeleteUser(int id)

        {
            try 
            {
                if (id == 0) 
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ResponseIsSuccessfull = false;
                    _response.ErrorMessages.Add("Invalid User");
                    _response.Result = id;
                    _loggerDev.Log($"Delete User: -- {id} -- can NOT be 0", "error");
                    return BadRequest(_response);
                }

                var getUser = await _appDBContext.Users.FirstOrDefaultAsync(user => user.Id ==id);

                if (getUser == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ResponseIsSuccessfull = false;
                    _response.ErrorMessages.Add("User NOT FOUND!");
                    _response.Result = getUser;
                    _loggerDev.Log($"Delete User: -- {id} -- NOT FOUND!", "error");
                    return NotFound(_response);
                }
                
                await _userRepository.RemoveUserAsync(getUser);
                _response.StatusCode = HttpStatusCode.OK;
                _response.ResponseIsSuccessfull = true;
                _response.ErrorMessages = null;
                _response.Result = null;
                _loggerDev.Log($"Delete User: User Removed Successfull!", "warning");
                return Ok(_response);

            }
            catch (Exception ex) 
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Exception thrown to Logs!");
                _loggerDev.Log(ex.Message.ToString(), "error");
                return BadRequest(_response);
            }
        }
  

        [HttpGet("AppUsers")]
        public async Task<ActionResult<IEnumerable<APIResponse>>> GetAppUsers()
        {
            var usersDTO = await _userRepository.GetAppUsersDTOAsync();

            if (usersDTO == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("User NOT FOUND!");
                _loggerDev.Log($"Get App Users: NO User Found!!!", "error");
                return NotFound(_response);
            }

            return Ok(usersDTO);
        }


        [HttpGet("SearchAppUser")]
        public async Task<ActionResult<APIResponse>> SearchAppUser( string searchByUserNameOrEmail = null)
        {
            // Retrieve all users
            var usersDTO = await _userRepository.GetAppUsersDTOAsync();

            // If no search term is provided, return all users
            if (string.IsNullOrWhiteSpace(searchByUserNameOrEmail))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Search Term is Empty!");
                _loggerDev.Log($"Search User: -- {searchByUserNameOrEmail} -- is EMPTY", "error");
                return BadRequest(_response);
            }

            if (usersDTO == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("User NOT FOUND!");
                _response.Result = null;
                _loggerDev.Log($"Search User: -- {searchByUserNameOrEmail} -- NOT FOUND!", "error");
                return BadRequest(_response);
            }

            try
            {
                // Otherwise, perform the search based on the provided search term
                var filteredUsersDTO = usersDTO
                    .Where(u =>
                        u.UserName.Contains(searchByUserNameOrEmail, StringComparison.OrdinalIgnoreCase) || // Search by user name
                        u.Email.Contains(searchByUserNameOrEmail, StringComparison.OrdinalIgnoreCase) // Search by email
                    );

                _response.StatusCode = HttpStatusCode.OK;
                _response.ResponseIsSuccessfull = true;
                _response.ErrorMessages = null;
                _response.Result = filteredUsersDTO;
                _loggerDev.Log($"Search User: {searchByUserNameOrEmail} FOUND", "info");
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ResponseIsSuccessfull = false;
                _response.ErrorMessages.Add("Exception thrown to Logs!");
                _loggerDev.Log(ex.Message.ToString(), "error");
                return BadRequest(_response);
            }
        }


        private byte[] ComputeHashValue(HMACSHA512 hmac, string objAndParam)
        {           
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(objAndParam));
            if(computedHash == null) 
            {
                _loggerDev.Log("Hash Value can not compute!!", "error");
            }
            _loggerDev.Log("Hash Value computed!!", "info");
            return computedHash;
        }

        private bool IsPasswordCorrect(string password)
        {
            var regexPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{8,}$";
            _loggerDev.Log("Check if User registeration password is matching with regex pattern", "info");
            return Regex.IsMatch(password, regexPattern) && password.Length >=8;
        }

        private bool IsEmailValid(string email)
        {
            // Regular expression pattern for email validation
            var regexPattern = @"^[a-zA-Z0-9+_.-]+@[a-zA-Z0-9.-]+$";

            // Log the email validation process
            _loggerDev.Log("Check if email format is valid", "info");

            // Check if the email matches the regex pattern and also meets the minimum length requirement
            return Regex.IsMatch(email, regexPattern) && email.Length >= 6;
        }

        private Task<string> GetuserNameFromEmail(RegisterDTO registerDTO) 
        {
            int atIndex = registerDTO.Email.IndexOf("@");
            string username = registerDTO.Email.Substring(0, atIndex);

            return Task.FromResult(username);
        }

        private Task<IQueryable<string>> GetEmailsByEmailAsync(string email)
        {
            _loggerDev.Log("QUERY Email column at Database ", "info");
            return Task.FromResult(_appDBContext.Users
                .Where(user => user.Email == email)
                .Select(user => user.Email));
        }

        private async Task<bool> IsUniqueUser(RegisterDTO registerDTO)
        {
            _loggerDev.Log("Check if Email is exist in DataBase", "info");
            var matchingEmailsQuery = await GetEmailsByEmailAsync(registerDTO.Email);
            var matchingEmails = await matchingEmailsQuery.ToListAsync();
            // If no appUser with the specified email exists, return true (email is unique)
            return !matchingEmails.Any();
        }

        private bool IsPhoneNumberValid(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return false;
            }

            string regPattern = @"^\d{10}$";

            if (!Regex.IsMatch(phoneNumber, regPattern))
            {
                return false;
            }

            return true;
        }
    }
}
