# MRTK3SpatialMesh - MeshDebugger para HoloLens 2

## Depurador Visual para Malhas Espaciais em Aplicações de Realidade Mista

Este projeto contém uma ferramenta para visualizar e depurar malhas espaciais em aplicações de realidade mista, destacando-as com um bounding box e exibindo informações detalhadas sobre cada malha.

## Sumário

- [Visão Geral](#visão-geral)
- [Requisitos](#requisitos)
- [Configuração do Projeto](#configuração-do-projeto)
- [Compilando para HoloLens 2](#compilando-para-hololens-2)
- [Depurando com Visual Studio](#depurando-com-visual-studio)
- [Como usar o MeshDebugger](#como-usar-o-meshdebuggger)
- [Funcionalidades](#funcionalidades)

## Visão Geral

O `MeshDebugger` é uma ferramenta que ajuda desenvolvedores a entender melhor as malhas espaciais geradas pelo HoloLens 2. Ele fornece:

- Visualização do bounding box em torno das malhas detectadas
- Detecção de malhas por raycast a partir do olhar do usuário
- Logs detalhados com informações sobre as malhas
- Estatísticas sobre vértices, triângulos e dimensões das malhas

## Requisitos

- Unity 2022.3 LTS ou superior
- Mixed Reality Toolkit 3 (MRTK3)
- Visual Studio 2022
- Windows 10/11 com Windows SDK 10.0.19041.0 ou superior
- HoloLens 2 (físico ou emulador)

## Configuração do Projeto

1. **Clone este repositório**

2. **Abra o projeto no Unity**
- Inicie o Unity Hub
- Clique em "Adicionar" e selecione a pasta do projeto
- Abra o projeto com a versão de Unity compatível

3. **Verifique a Configuração do MRTK3**
- Certifique-se de que o MRTK3 está importado corretamente
- Navegue até `Window > XR > MRTK3 > Project Validator` para verificar configurações

4. **Configure a Cena**
- Abra a cena principal que está na pasta Scenes.
  
## Compilando para HoloLens 2

1. **Configure a Plataforma para UWP**
- Vá para `File > Build Settings`
- Selecione `Universal Windows Platform`
- Clique em `Switch Platform`

2. **Defina Configurações de Build**
- Em Build Settings, configure:
  - Architecture: `ARM64`
  - Build Type: `D3D Project`
  - Build and Run on: `Local Machine`
  - Build configuration: `Debug`
  - Marque:
    - "Development Build"
    - "Script Debugging"
    - "Wait for Managed Debugger"

3. **Configure Player Settings**
- Clique em `Player Settings` no canto inferior da janela Build Settings
- Em `XR Plug-in Management`, habilite `OpenXR`
- Em `OpenXR Feature Groups`, habilite `Microsoft HoloLens`
- Em `Publishing Settings`, verifique:
  - Capabilities: `InternetClient`, `SpatialPerception`, `WebCam`

4. **Build do Projeto**
- Clique em `Build`
- Selecione uma pasta para salvar o projeto (crie uma pasta dedicada para builds)
- Aguarde o processo de build terminar

## Depurando com Visual Studio

1. **Abra a Solução no Visual Studio**
- Navegue até a pasta de build selecionada
- Abra o arquivo `.sln` (solução)

2. **Configure Opções de Depuração**
- Na barra de ferramentas, defina:
  - Configuração: `Debug`
  - Plataforma: `ARM64`
  - Dispositivo de destino: `Device` (para HoloLens físico) ou `Remote Machine` (para conexão remota)

3. **Configure a Conexão Remota** (se estiver usando Wi-Fi)
- Em Propriedades do Projeto > Debugging
- Em "Machine Name", insira o endereço IP do HoloLens
- Em "Authentication Type", selecione "Universal (Unencrypted Protocol)"

5. **Inicie a Depuração**
- Conecte o HoloLens 2 via USB ou configure a conexão via Wi-Fi
- Pressione F5 ou clique em `Debug > Start Debugging`
- O projeto será compilado, implantado e iniciado no HoloLens

6. **Para logs de debug**
- No Visual Studio, use `Debug > Windows > Output` para ver os logs
- Filtre os logs por `[MESH_DEBUGGER]` para ver apenas as mensagens do MeshDebugger

## Como usar o MeshDebuggger

1. **Colocação do Script**
- Adicione o script `MeshDebugger.cs` a um GameObject que tenha o componente `ARMeshManager`
- Ou adicione ambos os componentes a um único GameObject

2. **Configuração no Inspector**
- `ARMeshManager`: Referência ao componente ARMeshManager (auto-preenchido se estiver no mesmo objeto)
- `Log Interval`: Intervalo de tempo entre logs detalhados
- `Detailed Logs`: Habilite para ver logs mais detalhados (mantido como opção comentada)
- `Highlight Color`: Cor do bounding box
- `Line Thickness`: Espessura das linhas do bounding box
- `Wireframe Material`: Material para as linhas (opcional, fallback para shader padrão)
- `Raycast Interval`: Frequência de verificação para novas malhas via raycast

3. **Em Execução**
- Olhe para uma superfície para que o sistema de malhas espaciais a detecte
- A malha que estiver no centro da visão será destacada com um bounding box
- Informações detalhadas sobre a malha aparecerão nos logs de depuração
- Filtre no Visual Studio por `[MESH_DEBUGGER]` para encontrar os logs facilmente

## Funcionalidades

- **Detecção por Raycast**: Identifica automaticamente a malha que está no centro da visão do usuário
- **Visualização de Bounding Box**: Desenha um bounding box em torno da malha detectada
- **Logs Detalhados**: Mostra informações como:
  - Nome da malha
  - Posição, rotação e escala
  - Número de vértices e triângulos
  - Dimensões da malha
  Exemplos de logs:
  ```
  [MESH_DEBUGGER] === MALHA DESTACADA ===
    Nome: Mesh 47CF0D434D6E6D95-964AE7AEBFFF368A
    Posição: (0.00, 1.60, 0.00)
    Rotação: (0.00, 0.00, 0.00)
    Escala: (1.00, 1.00, 1.00)
    Vértices: 3483
    Triângulos: 5824
    Tamanho: (2.65, 2.52, 2.74)
    ```
- **Auto-atualização**: Se a malha destacada for removida, o sistema buscará automaticamente outra malha para destacar
- **Depuração Visual**: As linhas coloridas permitem visualizar facilmente o tamanho e posição das malhas

## Como Funciona

O script `MeshDebugger.cs` possui os seguintes componentes principais:

1. **Sistema de Raycast**: Lança um raio do centro da visão do usuário para detectar malhas
2. **Processamento de Malhas**: Captura e processa malhas do ARMeshManager, destacando aquela apontada pelo usuário
3. **Visualização**: Desenha linhas para formar um bounding box ao redor da malha selecionada
4. **Logging**: Exibe informações detalhadas sobre a malha para fins de depuração

## Testar com build já pronto
Na pasta que está o build, abra o arquivo `MeshDebugger.sln` e siga os passos comentados em [Depurando com Visual Studio](#depurando-com-visual-studio).
- Para ver os logs de depuração, vá em `Debug > Windows > Output` no visual studio e filtre por `[MESH_DEBUGGER]` para ver apenas as mensagens do MeshDebugger.
