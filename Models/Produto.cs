using System.ComponentModel.DataAnnotations;

namespace MinimalApiProdutos.Models
{
    public class Produto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Nome { get; set; } = string.Empty;

        public decimal Preco { get; set; }

        public int Estoque { get; set; }

        public int CategoriaId { get; set; }

        public Categoria Categoria { get; set; } = null!;
    }
}