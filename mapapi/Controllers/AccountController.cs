using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using mapapi.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace mapapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string connString;

        public AccountController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            connString = Environment.GetEnvironmentVariable("dbconnstring");
        }

        //[Authorize(Policy = "ValidAccessToken")]
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        /// <summary>
        /// Get detailed information about user
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified user was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpGet]
        [Route("[action]/{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();
            return user;
        }

        //[Route("[action]")]
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<User>>> GetSecret()
        //{
        //    var secret_str = _configuration.GetValue<string>(
        //        "AppSettings:Secret");
        //    return Ok(new
        //    {
        //        secret = secret_str
        //    });
        //}

        //[HttpGet("{id}")]
        //public async Task<ActionResult<User>> GetUser(long id)
        //{
        //    var user = await _context.Users.FirstOrDefaultAsync(i => i.IdUser == id);

        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    return user;
        //}

        /// <summary>
        /// Authentication. Returns WWW-Authenticate Bearer access token and refresh token.
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect</response>
        /// <response code="401">Username or password is incorrect</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpPost("Authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateModel model)
        {
            User user = _context.Users.FirstOrDefault(u => u.Username == model.Username);
            if (user != null)
            {
                byte[] salt = user.Salt;
                var pbkdf2 = new Rfc2898DeriveBytes(model.Password, salt, 100000);
                byte[] hash = pbkdf2.GetBytes(20);
                int i;
                for (i = 0; i < hash.Length; i++)
                {
                    if (hash[i] != user.Password[i])
                        break;
                }
                if (i == hash.Length)
                {
                    string encodedJwt = GenerateAccessToken(user);
                    //GUID is not random
                    //Guid guid = Guid.NewGuid();
                    //string refreshStr = guid.ToString();

                    string refreshStr = GenerateRefreshToken();
                    if (user.RefreshTokens == null)
                    {
                        user.RefreshTokens = new List<RefreshToken>();
                    }
                    user.RefreshTokens.Add(new RefreshToken
                    {
                        TokenStr = refreshStr,
                        Created = DateTime.UtcNow,
                        Expires = DateTime.UtcNow.AddDays(7)
                    });
                    _context.SaveChanges();
                    return Ok(new
                    {
                        IdUser = user.IdUser,
                        Username = user.Username,
                        Email = user.Email,
                        AccessToken = encodedJwt,
                        RefreshToken = refreshStr
                    });
                }
                else
                    //return BadRequest(new { message = "Username or password is incorrect" });
                    return Problem(statusCode: 401, detail: "Username or password is incorrect");
            }
            else
                //return BadRequest(new { message = "Username or password is incorrect" });
                return Problem(statusCode: 401, detail: "Username or password is incorrect");
        }

        /// <summary>
        /// Update access and refresh tokens pair. Use this method if access token expired
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Specified refresh token is incorrect</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpPost("RefreshToken")] //[action]/{refreshTokenStr}
        public IActionResult RefreshToken([Required] string refreshTokenStr)
        {
            var user = _context.Users
                    .SingleOrDefault(s => s.RefreshTokens
                        .Any(t => t.TokenStr == refreshTokenStr));
            if(user == null)
                return Problem(statusCode: 401, detail: "Invalid token");
            var refreshToken = _context.RefreshTokens.Single(s => s.TokenStr == refreshTokenStr);
            if(DateTime.UtcNow >= refreshToken.Expires || refreshToken.Revoked != null)
                return Problem(statusCode: 401, detail: "Invalid token");

            string newRefreshTokenStr = GenerateRefreshToken();
            user.RefreshTokens.Add(new RefreshToken
            {
                TokenStr = newRefreshTokenStr,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            });
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.ReplacedBy = newRefreshTokenStr;
            _context.SaveChanges();
            string newEncodedJwt = GenerateAccessToken(user);
            return Ok(new
            {
                IdUser = user.IdUser,
                Username = user.Username,
                Email = user.Email,
                AccessToken = newEncodedJwt,
                RefreshToken = newRefreshTokenStr
            });
        }

        private string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim("id", user.IdUser.ToString()),
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)
            };
            ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            var currDate = DateTime.UtcNow;
            var key = _configuration.GetValue<string>("AppSettings:Secret");
            var jwt = new JwtSecurityToken(
                issuer: "MapApiServer",
                audience: "MapApiClient",
                notBefore: currDate,
                claims: claimsIdentity.Claims,
                expires: currDate.Add(TimeSpan.FromMinutes(30)), 
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                    SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        private string GenerateRefreshToken()
        {
            var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            string refreshStr = Convert.ToBase64String(randomBytes);
            return refreshStr;
        }

        //private ClaimsIdentity GetIdentity(string username, string password)
        //{
        //    Person person = people.FirstOrDefault(x => x.Login == username && x.Password == password);
        //    if (person != null)
        //    {
        //        var claims = new List<Claim>
        //        {
        //            new Claim(ClaimsIdentity.DefaultNameClaimType, person.Login),
        //            new Claim(ClaimsIdentity.DefaultRoleClaimType, person.Role)
        //        };
        //        ClaimsIdentity claimsIdentity =
        //        new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
        //            ClaimsIdentity.DefaultRoleClaimType);
        //        return claimsIdentity;
        //    }

        //    // если пользователя не найдено
        //    return null;
        //}

        /// <summary>
        /// Registration. Add new user
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(model.Password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);

            //string salt_str = System.Text.Encoding.UTF8.GetString(salt, 0, salt.Length);
            //string hash_str = System.Text.Encoding.Unicode.GetString(hash, 0, hash.Length);
            User new_user = new User { 
                Username = model.Username, 
                Email = model.Email, 
                Fullname = model.Fullname,
                BirthDate = model.BirthDate,
                CreatedDate = DateTime.UtcNow,
                Password = hash, 
                Salt = salt, 
                Role="user"
            };

            _context.Users.Add(new_user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                IdUser = new_user.IdUser,
                Username = new_user.Username,
                Email = new_user.Email
            });

        }

        //[HttpPost]
        //public async Task<ActionResult<User>> PostUser(User user)
        //{
        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetOUser", new { id = user.IdUser }, user);
        //}

        //private bool UserExists(long id)
        //{
        //    return _context.Users.Any(e => e.IdUser == id);
        //}

        // PUT: api/Users/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.

        /// <summary>
        /// Edit information about user
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///         "idUser": 1,
        ///         "username": "qwerty",
        ///         "fullname": "Tom Rose",
        ///         "birthDate": "2000-06-06",
        ///         "createdDate": "2021-06-06T20:21:41",
        ///         "role": "user"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Not allowed to edit info about this user</response>
        /// <response code="404">Specified user was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<User>> PutUser(int id, User user)
        {
            string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            int userId = Int32.Parse(User.FindFirst("id").Value);
            if (id != user.IdUser)
            {
                return BadRequest();
            }
            try
            {
                if (user.IdUser != userId)
                    return Forbid();
                else
                {
                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return user;
        }

        // DELETE: api/Users/5
        /// <summary>
        /// Delete information about user
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Not allowed to edit info about this user</response>
        /// <response code="404">Specified user was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(int id)
        {
            var authorizedUser = User.Identity.Name;
            var user = await _context.Users.FindAsync(id);
            if(authorizedUser != user.Username)
            {
                return Forbid();
            }
            else
            {
                if (user == null)
                {
                    return NotFound();
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return user;
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.IdUser == id);
        }
    }
}
