namespace ZeroToHero.Core.Interfaces
{
    /// <summary>
    /// Interface para entidades que podem receber dano.
    /// Implemente em qualquer classe que precise responder a ataques.
    /// </summary>
    public interface IDanificavel
    {
        /// <summary>
        /// Aplica dano à entidade.
        /// </summary>
        /// <param name="dano">Quantidade de dano a ser aplicada.</param>
        void ReceberDano(float dano);

        /// <summary>
        /// Recupera vida da entidade.
        /// </summary>
        /// <param name="quantidade">Quantidade de vida a ser recuperada.</param>
        void Curar(float quantidade);

        /// <summary>
        /// Chamado quando a vida da entidade chega a 0.
        /// </summary>
        void Morrer();
    }
}
