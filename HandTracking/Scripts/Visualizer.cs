using UnityEngine;
using UnityEngine.UI;
using MediaPipe.BlazePalm;
using Klak.TestTools;

namespace MediaPipe {

    public class Visualizer : MonoBehaviour {
        #region Editable attributes

        [SerializeField] protected ImageSource source;
        [SerializeField] RawImage _previewUI = null;
        [Space]
        [SerializeField] ResourceSet _resources = null;
        [SerializeField] Shader _shader = null;

        #endregion

        #region Private members

        PalmDetector _detector;
        Material _material;

        ComputeBuffer _boxDrawArgs;
        ComputeBuffer _keyDrawArgs;

        #endregion

        #region MonoBehaviour implementation

        void Start() {
            _detector = new PalmDetector(_resources);
            _material = new Material(_shader);

            var cbType = ComputeBufferType.IndirectArguments;
            _boxDrawArgs = new ComputeBuffer(4, sizeof(uint), cbType);
            _keyDrawArgs = new ComputeBuffer(4, sizeof(uint), cbType);
            _boxDrawArgs.SetData(new[] { 6, 0, 0, 0 });
            _keyDrawArgs.SetData(new[] { 24, 0, 0, 0 });
        }

        void OnDestroy() {
            _detector.Dispose();
            Destroy(_material);

            _boxDrawArgs.Dispose();
            _keyDrawArgs.Dispose();
        }
        void LateUpdate() {
            _detector.ProcessImage(source.Texture);
            _previewUI.texture = source.Texture;
        }

        void OnRenderObject() {
            // Detection buffer
            _material.SetBuffer("_Detections", _detector.DetectionBuffer);

            // Copy the detection count into the indirect draw args.
            _detector.SetIndirectDrawCount(_boxDrawArgs);
            _detector.SetIndirectDrawCount(_keyDrawArgs);
            var d = new int[4];
            _boxDrawArgs.GetData(d);

            var ratio = new Vector2(_previewUI.rectTransform.sizeDelta.x / Screen.width, _previewUI.rectTransform.sizeDelta.y / Screen.height);
            _material.SetVector("_CanvasRatio", ratio);
            _material.SetVector("_CanvasOffset", new Vector2(1 - ratio.x, 1 - ratio.y));

            // Bounding box
            _material.SetPass(0);
            Graphics.DrawProceduralIndirectNow(MeshTopology.Triangles, _boxDrawArgs, 0);

            // Key points
            _material.SetPass(1);
            Graphics.DrawProceduralIndirectNow(MeshTopology.Lines, _keyDrawArgs, 0);
        }

        #endregion
    }

} // namespace MediaPipe
