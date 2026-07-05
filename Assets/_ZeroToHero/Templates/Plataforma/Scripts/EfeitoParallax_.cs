using UnityEngine;

namespace ZeroToHero.Cenas.Plataforma
{
    public class FundoInfinito : MonoBehaviour
    {
        [SerializeField] private float velocidadeParallax = 0.5f;

        private Transform _cameraTransform;
        private Vector3 _ultimaPosCamera;
        private float _larguraImagem;

        private void Start()
        {
            _cameraTransform = Camera.main.transform;
            _ultimaPosCamera = _cameraTransform.position;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            _larguraImagem = sr.bounds.size.x;
        }

        private void LateUpdate()
        {
            Vector3 movimentoCamera = _cameraTransform.position - _ultimaPosCamera;
            transform.position += new Vector3(movimentoCamera.x * velocidadeParallax, 0f, 0f);
            _ultimaPosCamera = _cameraTransform.position;

            if (Mathf.Abs(_cameraTransform.position.x - transform.position.x) >= _larguraImagem)
            {
                float reposicionamento = (_cameraTransform.position.x - transform.position.x) % _larguraImagem;
                transform.position = new Vector3(_cameraTransform.position.x + reposicionamento, transform.position.y, transform.position.z);
            }
        }
    }
}