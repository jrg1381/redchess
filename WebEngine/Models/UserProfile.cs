using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedChess.WebEngine.Models
{
    [Table("UserProfile")]
    public class UserProfile
    {
        private string m_UnderlyingUsername;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        public string UserName
        {
            get { return m_UnderlyingUsername; }
            set { m_UnderlyingUsername = value.ToLowerInvariant(); }
        }

        public string EmailHash { get; set; }
    }
}