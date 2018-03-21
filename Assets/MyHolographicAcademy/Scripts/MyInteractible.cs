using UnityEngine;

namespace EDUCATION.FEELPHYSICS.MY_HOLOGRAPHIC_ACADEMY
{
    /// <summary>
    /// このクラスは GameObject をインタラクト可能にする。gaze されたとき何が起きるかを決定する。
    /// </summary>
    public class MyInteractible : MonoBehaviour
    {
        #region Public Valiables

        [Tooltip("ホログラムにインタラクトしたときに鳴らすオーディオクリップ")]
        public AudioClip TargetFeedbackSound;

        #endregion

        #region Private Valiables

        /// <summary>
        /// フォーカスしたときにオーディオクリップを再生するためのもの
        /// </summary>
        private AudioSource audioSource;

        /// <summary>
        /// フォーカスする対象の GameObject に当たっているマテリアルの配列
        /// </summary>
        private Material[] defaultMaterials;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// もし Collider がなければ追加する
        /// ＋オーディオクリップを再生する準備をする
        /// </summary>
        private void Start()
        {
            this.defaultMaterials = GetComponent<Renderer>().materials;

            // もし Collider がなければ追加する
            Collider collider = GetComponentInChildren<Collider>();
            if (collider == null)
            {
                gameObject.AddComponent<BoxCollider>();
            }

            this.EnableAudioHapticFeedback();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// gaze された瞬間に呼ばれる
        /// </summary>
        public void GazeEntered()
        {
            for (int i = 0; i < this.defaultMaterials.Length; i++)
            {
                this.SetColorWithEmissionGamma(this.defaultMaterials[i], .02f);
            }

            if (this.audioSource != null && !this.audioSource.isPlaying)
            {
                this.audioSource.Play();
            }
        }

        /// <summary>
        /// gaze が外れた瞬間に呼ばれる
        /// </summary>
        public void GazeExited()
        {
            for (int i = 0; i < this.defaultMaterials.Length; i++)
            {
                this.SetColorWithEmissionGamma(this.defaultMaterials[i], 0f);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// オーディオクリップを再生するための AudioSource を追加する
        /// </summary>
        private void EnableAudioHapticFeedback()
        {
            if (this.TargetFeedbackSound != null)
            {
                this.audioSource = this.GetComponent<AudioSource>();
                if (this.audioSource == null)
                {
                    this.audioSource = this.gameObject.AddComponent<AudioSource>();
                }

                this.audioSource.clip = this.TargetFeedbackSound;
                this.audioSource.playOnAwake = false;
                this.audioSource.spatialBlend = 1;
                this.audioSource.dopplerLevel = 0;
            }
        }

        /// <summary>
        /// マテリアルを明るくする
        /// </summary>
        /// <param name="material">明るくするマテリアル</param>
        /// <param name="gamma">どれくらい明るくするか</param>
        private void SetColorWithEmissionGamma(Material material, float gamma)
        {
            var baseColor = material.color;
            Color finalColor = baseColor * Mathf.LinearToGammaSpace(gamma);
            material.SetColor("_EmissionColor", finalColor);
        }

        #endregion
    }
}