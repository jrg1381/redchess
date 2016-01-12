using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace RedChess.WebEngine.Models
{
    [Table("UserProfile")]
    public class UserProfile
    {
        private string m_underlyingUsername;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        public string UserName
        {
            get { return m_underlyingUsername; }
            set { m_underlyingUsername = value.ToLowerInvariant(); }
        }

        public string EmailHash { get; set; }
    }
}