using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MinimalApiProdutos.Models
{
    /// <summary>
    /// Representa uma categoria de produtos.
    /// </summary>
    public class Categoria
    {
        /// <summary>
        /// Identificador único da categoria.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome da categoria. Obrigatório e limitado a 100 caracteres.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Lista de produtos associados a esta categoria.
        /// </summary>
        [JsonIgnore]
        public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
    }
}