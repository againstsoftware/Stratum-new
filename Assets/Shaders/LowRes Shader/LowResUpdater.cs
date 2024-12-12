using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class LowResUpdater : MonoBehaviour
{
    [FormerlySerializedAs("shaderMaterial")] [SerializeField]
    private Material _shaderMaterial; // Asigna tu material en el inspector

    [FormerlySerializedAs("scaledScreenHeight")] [SerializeField]
    private int _scaledScreenHeight = 600; // Altura de pantalla deseada (editable en el inspector)

    [SerializeField] private bool _logOnUpdate;

    private int _lastScreenWidth, _lastScreenHeight;
    private int _lastScaledScreenHeight;


    private static readonly int _blockCountHash = Shader.PropertyToID("_Block_Count");
    private static readonly int _blockSizeHash = Shader.PropertyToID("_Block_Size");
    private static readonly int _halfBlockSizeHash = Shader.PropertyToID("_Half_Block_Size");

    private void Start()
    {
        UpdateResolutionScale(); // Asegúrate de calcular al inicio
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            // Detecta cambios en el modo de juego
            if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight ||
                _scaledScreenHeight != _lastScaledScreenHeight)
            {
                UpdateResolutionScale();
            }
        }
    }

    private void OnValidate()
    {
        // Actualiza en modo edición cuando cambie `screenHeight` o el material
        if (!Application.isPlaying)
        {
            UpdateResolutionScale();
        }
    }

    private void UpdateResolutionScale()
    {
        // Obtiene el tamaño de la pantalla según el contexto
        Vector2 screenSize = Application.isPlaying ? new Vector2(Screen.width, Screen.height) : GetEditorGameViewSize();

        // Guarda las dimensiones previas para evitar cálculos innecesarios
        _lastScreenWidth = (int)screenSize.x;
        _lastScreenHeight = (int)screenSize.y;
        _lastScaledScreenHeight = _scaledScreenHeight;

        // Calcula la escala de resolución
        float aspectRatio = screenSize.x / screenSize.y;
        float scaledWidth = _scaledScreenHeight * aspectRatio;

        float scaledScreenWidth = (int)(_scaledScreenHeight * aspectRatio + 0.5f);

        Vector2 blockCount = new Vector2(scaledScreenWidth, _scaledScreenHeight);
        Vector2 blockSize = new Vector2(1f / scaledScreenWidth, 1f / _scaledScreenHeight);
        Vector2 halfBlockSize = new Vector2(.5f / scaledScreenWidth, .5f / _scaledScreenHeight);


        // Pasa los valores al material si está asignado
        if (_shaderMaterial is not null)
        {
            _shaderMaterial.SetVector(_blockCountHash, blockCount);
            _shaderMaterial.SetVector(_blockSizeHash, blockSize);
            _shaderMaterial.SetVector(_halfBlockSizeHash, halfBlockSize);
        }

        // Log para depuración
        if (_logOnUpdate) Debug.Log($"Updated Low Res. (Screen: {screenSize})");
    }

    private Vector2 GetEditorGameViewSize()
    {
#if UNITY_EDITOR
        System.Type gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        if (gameViewType == null) return new Vector2(1920, 1080); // Valor predeterminado si no se encuentra el GameView

        // Usar reflexión para obtener el tamaño del Game View sin abrir ventanas
        var getSizeMethod = gameViewType.GetMethod("GetMainGameViewSize",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (getSizeMethod == null) return new Vector2(1920, 1080);

        return (Vector2)getSizeMethod.Invoke(null, null);
#else
return Vector2.zero;
#endif
    }
}