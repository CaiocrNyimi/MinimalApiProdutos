using System.ComponentModel.DataAnnotations;

namespace MinimalApiProdutos.Models
{
    /// <summary>
    /// Representa um produto disponível no sistema.
    /// </summary>
    public class Produto
    {
        /// <summary>
        /// Identificador único do produto.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome do produto. Obrigatório e limitado a 150 caracteres.
        /// </summary>
        [Required]
        [StringLength(150)]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Preço do produto.
        /// </summary>
        public decimal Preco { get; set; }

        /// <summary>
        /// Quantidade disponível em estoque.
        /// </summary>
        public int Estoque { get; set; }

        /// <summary>
        /// Identificador da categoria à qual o produto pertence.
        /// </summary>
        public int CategoriaId { get; set; }

        /// <summary>
        /// Categoria associada ao produto.
        /// </summary>
        public Categoria Categoria { get; set; } = null!;
    }
}