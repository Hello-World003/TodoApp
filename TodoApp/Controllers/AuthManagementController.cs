using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TodoApp.Configuration;
using TodoApp.Data;
using TodoApp.Models.Dtos;
using TodoApp.Models.Dtos.Requests;
using TodoApp.Models.Dtos.Responses;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace TodoApp.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class AuthManagementController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;
        private readonly TokenValidationParameters _validationParameters;
        private readonly ApiDbContext _apiDbContext;

        public AuthManagementController(UserManager<IdentityUser> userManager, JwtConfig jwtConfig, TokenValidationParameters validationParameters, ApiDbContext apiDbContext)
        {
            _userManager = userManager;
            _jwtConfig = jwtConfig;
            _validationParameters = validationParameters;
            _apiDbContext = apiDbContext;
        }
        
        


        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto)
        {
            if (ModelState.IsValid)
            {
                //We can utilise the model

                var existingUser = await _userManager.FindByEmailAsync(userRegisterDto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Erros = new List<string>()
                        {
                            "Invalid Payload"
                    
                        },
                        Success = false,
                
                    });
                }

                var newUser = new IdentityUser()
                {
                    Email = userRegisterDto.Email,
                    UserName = userRegisterDto.UserName,
                };


                var isCreated = await _userManager.CreateAsync(newUser, userRegisterDto.Password);


                if (isCreated.Succeeded)
                {
                    var resultToken = GenerateJwtToken(newUser) ;

                    return Ok(new RegistrationResponse()
                    {
                        Success = true,
                        Token = resultToken.ToString()
                    });
                }
                else
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Erros = isCreated.Errors.Select(x => x.Description).ToList(),
                        Success = false,
                    });
                }
                
            }

            return BadRequest(new RegistrationResponse()
            {
                Erros = new List<string>()
                {
                    "Invalid Payload"
                    
                },
                Success = false,
                
            });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest userLogin)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(userLogin.Email);

                if (existingUser == null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Erros = new List<string>()
                        {
                            "Invalid Login Request"
                        },
                        Success = false
                    });
                }

                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, userLogin.Password);
                if (!isCorrect)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Erros = new List<string>()
                        {
                            "Invalid Login Request"
                        },
                        Success = false
                    });
                }

                var jwtToken = await GenerateJwtToken(existingUser);

                return Ok(new RegistrationResponse()
                {
                    Success = true,
                    Token = jwtToken.ToString()
                });
            }

            return BadRequest(new RegistrationResponse()
            {
                Erros = new List<string>()
                {
                    "Invalid Payload"
                },
                Success = false
            });
        }

        private async Task<AuthResult> GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddSeconds(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken()
            {
                Id = token.Id,
                isUsed = false,
                isRevorked = false,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                Token = RandomString(35) + Guid.NewGuid()
            };

            await _apiDbContext.RefreshTokens.AddAsync(refreshToken);
            await _apiDbContext.SaveChangesAsync();

            return new AuthResult()
            {
                Token = jwtToken,
                Success = true,
                RefreshToken = refreshToken.Token
            };
        }

        private string RandomString(int i)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVXUZW0123456789";

            return new string(Enumerable.Repeat(chars, chars.Length)
                .Select(x => x[random.Next(x.Length)]).ToArray());
        }
    }
}