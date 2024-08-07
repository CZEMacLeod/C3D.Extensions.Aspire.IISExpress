using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF6WebApp.Models;
public partial class Blog
{
    public Blog()
    {
    }   

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BlogId { get; set; }

    [StringLength(200), Required, Index()]
    public string Name { get; set; }

    [StringLength(200)]
    public string Url { get; set; }

}