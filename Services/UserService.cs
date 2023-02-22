﻿using API.Models;
using API.Models.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static API.Services.UserService;
using API.Services;
using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Data;


namespace API.Services
{
    public class UserService
    {
        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO, B2bapiContext _db, 
            string secretKey, IMapper _mapper)
        {
            //check Username 
            string requestUsername = loginRequestDTO.UserName;
            String userRole;
            bool isValid;
            User user = _db.Users.FirstOrDefault(u => u.LoginId == loginRequestDTO.UserName);

            //check id 
            if (user == null || string.IsNullOrEmpty(user.LoginId))
            {
                isValid = false;
                return new LoginResponseDTO()
                {
                    Token = "",
                    User = null,
                };
            }
            else
            {
                isValid = true;
                //role
                if (!string.IsNullOrEmpty(user.LoginId) && user.IsAdmin)
                {
                    userRole = "admin";
                }
                else
                {
                    userRole = "subAccount";
                }
            }



          

            //check password     
            if (user.Password == loginRequestDTO.Password)
            {
                isValid = true;
            }
            else
            {
                isValid = false;
            }



            //if not valid
            if (user == null || isValid == false)
            {
                return new LoginResponseDTO()
                {
                    Token = "",
                    User = null,
                };
            }


            //if user was found generate JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                         {
                    new Claim(ClaimTypes.Name, user.LoginId),
                    new Claim(ClaimTypes.Role, userRole)
                         }),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

           

            //Actually generate token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
            {
                Token = tokenHandler.WriteToken(token),
                User = _mapper.Map<UserDTO>(user),
            };


            return loginResponseDTO;
        }

    }
}