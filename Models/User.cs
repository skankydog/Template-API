using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Models
{
    public class User : IdentityUser<int>  // remove the dynamic type later and use a Guid instead
    {
        //public int Id { get; set; } ---- comes with IdentityUser
        //public string Username { get; set; } ---- comes with IdentityUser
        //public byte[] PasswordHash { get; set; } ---- comes with IdentityUser
        //public byte[] PasswordSalt { get; set; } ---- comes with IdentityUser
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Introduction { get; set; }
        public string Interests { get; set; }
        public string Description { get; set; }
        public string LookingFor { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } // identity


// need virtual when using lazy loading...
        public virtual ICollection<Photo> Photos { get; set; } // need virtual when using lazy loading

        public virtual  ICollection<Like> Likers { get; set; } // need virtual when using lazy loading
        public virtual  ICollection<Like> Likees { get; set; } // need virtual when using lazy loading
        public virtual  ICollection<Message> MessagesSent { get; set; } // need virtual when using lazy loading
        public virtual  ICollection<Message> MessagesReceived { get; set; } // added virtual to make use of lazy loading
    }
}