using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using System.Text;

public class MeshDebugger : MonoBehaviour
{
    // Constantes para tags de log
    private const string LOG_TAG = "[MESH_DEBUGGER]"; // Tag única para filtrar no console

    [SerializeField]
    private ARMeshManager meshManager;

    [SerializeField]
    private float logInterval = 2.0f;  // Intervalo em segundos para evitar spam de logs

    [SerializeField]
    private bool detailedLogs = false;  // Se true, mostra informações detalhadas sobre cada malha

    [SerializeField]
    private Color highlightColor = Color.yellow;  // Cor das linhas de destaque

    [SerializeField]
    private float lineThickness = 0.01f;  // Espessura da linha (em metros)

    [SerializeField]
    private Material wireframeMaterial; // Arraste o MRTK_WireframeBlue para esta referência no Inspector

    [SerializeField]
    private float raycastInterval = 0.5f;  // Intervalo em segundos entre tentativas de raycast

    private float nextRaycastTime = 0f;
    private StringBuilder logBuilder = new StringBuilder();
    private MeshFilter highlightedMesh;

    // Contador para estatísticas (mantidos para referência futura)
    private int totalMeshes = 0;
    private int addedMeshes = 0;
    private int updatedMeshes = 0;
    private int removedMeshes = 0;

    // Objetos para desenhar linhas 3D
    private GameObject lineContainer;
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();

    void Start()
    {
        if (meshManager == null)
            meshManager = GetComponent<ARMeshManager>();

        if (meshManager != null)
        {
            meshManager.meshesChanged += OnMeshesChanged;
            UnityEngine.Debug.Log($"{LOG_TAG} Inicializado com sucesso!");
        }
        else
        {
            UnityEngine.Debug.LogError($"{LOG_TAG} ARMeshManager não encontrado!");
        }

        // Criar container para linhas de destaque
        lineContainer = new GameObject("HighlightLineContainer");
        lineContainer.transform.SetParent(transform);

        // Iniciar com raycast imediatamente
        nextRaycastTime = 0f;
    }

    void Update()
    {
        if (meshManager == null || meshManager.meshes.Count == 0)
            return;

        // Verificar se devemos fazer raycast para encontrar malha na visão
        if (Time.time >= nextRaycastTime)
        {
            if (FindMeshInView())
            {
                nextRaycastTime = Time.time + raycastInterval;
            }
            else
            {
                // Se não encontrar uma malha, tentar novamente mais rapidamente
                nextRaycastTime = Time.time + 0.2f;
            }
        }
    }

    private void OnMeshesChanged(ARMeshesChangedEventArgs args)
    {
        // Atualizar estatísticas (mantidas para referência futura)
        addedMeshes += args.added.Count;
        updatedMeshes += args.updated.Count;
        removedMeshes += args.removed.Count;
        totalMeshes = meshManager.meshes.Count;

        // Verificar se a malha destacada foi removida
        if (highlightedMesh != null && args.removed.Contains(highlightedMesh))
        {
            UnityEngine.Debug.Log($"{LOG_TAG} A malha destacada foi removida.");
            highlightedMesh = null;
            ClearHighlightLines();
        }

        /* Comentado para mostrar apenas logs da malha destacada
        // Só registramos logs completos no intervalo definido para evitar spam
        if (Time.time >= nextLogTime)
        {
            LogMeshInfo(args);
            nextLogTime = Time.time + logInterval;
        }
        */
    }

    // Novo método para encontrar malha no campo de visão
    private bool FindMeshInView()
    {
        // Obter a câmera principal (visão do usuário)
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return false;

        // Lançar raio do centro da tela
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Verificar se o raio atingiu algo
        if (Physics.Raycast(ray, out hit))
        {
            // Verificar se o objeto atingido tem um MeshFilter
            MeshFilter hitMeshFilter = hit.collider.GetComponent<MeshFilter>();

            // Se encontramos um MeshFilter e é diferente do atual
            if (hitMeshFilter != null && hitMeshFilter != highlightedMesh)
            {
                // Verificar se está na lista de malhas do ARMeshManager
                if (meshManager.meshes.Contains(hitMeshFilter))
                {
                    // Limpar linhas anteriores
                    ClearHighlightLines();

                    // Definir a nova malha destacada
                    highlightedMesh = hitMeshFilter;

                    // Desenhar linhas de destaque
                    DrawBoundsAroundMesh(highlightedMesh);

                    // Log detalhado da malha destacada
                    LogHighlightedMeshInfo();

                    return true;
                }
            }
        }

        return false;
    }

    // Método corrigido para desenhar o bounding box na escala correta
    private void DrawBoundsAroundMesh(MeshFilter meshFilter)
    {
        if (meshFilter == null || meshFilter.sharedMesh == null) return;

        Mesh mesh = meshFilter.sharedMesh;
        Bounds localBounds = mesh.bounds;

        // Calcular a escala correta para o bounding box
        // A matriz de transformação local para global
        Matrix4x4 localToWorldMatrix = meshFilter.transform.localToWorldMatrix;

        // Obter o centro da bounding box no mundo
        Vector3 worldCenter = meshFilter.transform.TransformPoint(localBounds.center);

        // Criar os 8 vértices do bounding box em coordenadas locais
        Vector3[] vertices = new Vector3[8];

        // Calcular metade do tamanho em cada dimensão
        float x = localBounds.extents.x;
        float y = localBounds.extents.y;
        float z = localBounds.extents.z;

        // Definir os 8 cantos do cubo local
        vertices[0] = localBounds.center + new Vector3(-x, -y, -z);  // canto inferior esquerdo traseiro
        vertices[1] = localBounds.center + new Vector3(x, -y, -z);   // canto inferior direito traseiro
        vertices[2] = localBounds.center + new Vector3(x, -y, z);    // canto inferior direito frontal
        vertices[3] = localBounds.center + new Vector3(-x, -y, z);   // canto inferior esquerdo frontal
        vertices[4] = localBounds.center + new Vector3(-x, y, -z);   // canto superior esquerdo traseiro
        vertices[5] = localBounds.center + new Vector3(x, y, -z);    // canto superior direito traseiro
        vertices[6] = localBounds.center + new Vector3(x, y, z);     // canto superior direito frontal
        vertices[7] = localBounds.center + new Vector3(-x, y, z);    // canto superior esquerdo frontal

        // Transformar cada vértice do espaço local para o mundial
        Vector3[] worldVertices = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            worldVertices[i] = meshFilter.transform.TransformPoint(vertices[i]);
        }

        // Desenhar as 12 linhas que compõem o bounding box
        // Base inferior
        CreateLine(worldVertices[0], worldVertices[1]);
        CreateLine(worldVertices[1], worldVertices[2]);
        CreateLine(worldVertices[2], worldVertices[3]);
        CreateLine(worldVertices[3], worldVertices[0]);

        // Base superior
        CreateLine(worldVertices[4], worldVertices[5]);
        CreateLine(worldVertices[5], worldVertices[6]);
        CreateLine(worldVertices[6], worldVertices[7]);
        CreateLine(worldVertices[7], worldVertices[4]);

        // Pilares conectando as bases
        CreateLine(worldVertices[0], worldVertices[4]);
        CreateLine(worldVertices[1], worldVertices[5]);
        CreateLine(worldVertices[2], worldVertices[6]);
        CreateLine(worldVertices[3], worldVertices[7]);

        // Adicionar linha central para orientação
        CreateLine(worldCenter, worldCenter + Vector3.up * 0.1f);
    }

    private void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("HighlightLine");
        lineObj.transform.SetParent(lineContainer.transform);

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.startWidth = lineThickness;
        line.endWidth = lineThickness;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        // Usar o material MRTK_WireframeBlue pré-configurado
        if (wireframeMaterial != null)
        {
            line.material = wireframeMaterial;
        }
        else
        {
            // Tentar carregar o material do MRTK se não foi referenciado no Inspector
            wireframeMaterial = Resources.Load<Material>("Materials/MRTK_WireframeBlue");

            if (wireframeMaterial != null)
            {
                line.material = wireframeMaterial;
            }
            else
            {
                // Fallback para shader compatível em caso do material não ser encontrado
                UnityEngine.Debug.LogWarning($"{LOG_TAG} Material MRTK_WireframeBlue não encontrado, usando shader alternativo.");

                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

                if (shader == null)
                    shader = Shader.Find("Standard");

                if (shader == null)
                    shader = Shader.Find("Hidden/InternalErrorShader");

                if (shader != null)
                {
                    Material lineMaterial = new Material(shader);
                    lineMaterial.color = highlightColor;
                    line.material = lineMaterial;
                }
            }
        }

        lineRenderers.Add(line);
    }

    private void ClearHighlightLines()
    {
        foreach (var line in lineRenderers)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        lineRenderers.Clear();
    }

    private void LogHighlightedMeshInfo()
    {
        if (highlightedMesh == null || highlightedMesh.sharedMesh == null) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"{LOG_TAG} === MALHA DESTACADA ===");
        sb.AppendLine($"Nome: {highlightedMesh.name}");
        sb.AppendLine($"Posição: {FormatVector3(highlightedMesh.transform.position)}");
        sb.AppendLine($"Rotação: {FormatVector3(highlightedMesh.transform.eulerAngles)}");
        sb.AppendLine($"Escala: {FormatVector3(highlightedMesh.transform.lossyScale)}");

        Mesh mesh = highlightedMesh.sharedMesh;
        sb.AppendLine($"Vértices: {mesh.vertexCount}");
        sb.AppendLine($"Triângulos: {mesh.triangles.Length / 3}");
        sb.AppendLine($"Tamanho: {FormatVector3(mesh.bounds.size)}");

        UnityEngine.Debug.Log(sb.ToString());
    }

    /* Comentado conforme solicitado - manter para referência futura
    private void LogMeshInfo(ARMeshesChangedEventArgs args)
    {
        logBuilder.Clear();
        logBuilder.AppendLine($"{LOG_TAG} {System.DateTime.Now.ToString("HH:mm:ss.fff")} - Estatísticas da Malha:");
        logBuilder.AppendLine($"Total de malhas: {totalMeshes}");
        logBuilder.AppendLine($"Malhas adicionadas (desde início): {addedMeshes}");
        logBuilder.AppendLine($"Malhas atualizadas (desde início): {updatedMeshes}");
        logBuilder.AppendLine($"Malhas removidas (desde início): {removedMeshes}");

        // Se logs detalhados estiver ativado, mostramos informações de cada malha
        if (detailedLogs)
        {
            // Processar malhas adicionadas
            if (args.added.Count > 0)
            {
                logBuilder.AppendLine($"\nMalhas adicionadas neste frame: {args.added.Count}");
                foreach (var mesh in args.added)
                {
                    LogMeshDetails(mesh);
                }
            }

            // Processar malhas atualizadas
            if (args.updated.Count > 0)
            {
                logBuilder.AppendLine($"\nMalhas atualizadas neste frame: {args.updated.Count}");
                foreach (var mesh in args.updated)
                {
                    LogMeshDetails(mesh);
                }
            }

            // Processar malhas removidas
            if (args.removed.Count > 0)
            {
                logBuilder.AppendLine($"\nMalhas removidas neste frame: {args.removed.Count}");
                foreach (var mesh in args.removed)
                {
                    logBuilder.AppendLine($"- {mesh.name} em {FormatVector3(mesh.transform.position)}");
                }
            }
        }

        UnityEngine.Debug.Log(logBuilder.ToString());
    }

    private void LogMeshDetails(MeshFilter meshFilter)
    {
        if (meshFilter == null || meshFilter.sharedMesh == null) return;

        logBuilder.AppendLine($"- {meshFilter.name}");
        logBuilder.AppendLine($"  Posição: {FormatVector3(meshFilter.transform.position)}");
        logBuilder.AppendLine($"  Escala: {FormatVector3(meshFilter.transform.lossyScale)}");
    }
    */

    private string FormatVector3(Vector3 vector)
    {
        return $"({vector.x:F2}, {vector.y:F2}, {vector.z:F2})";
    }

    void OnDisable()
    {
        ClearHighlightLines();

        if (meshManager != null)
        {
            meshManager.meshesChanged -= OnMeshesChanged;
        }
    }

    void OnDestroy()
    {
        ClearHighlightLines();

        if (lineContainer != null)
            Destroy(lineContainer);
    }
}