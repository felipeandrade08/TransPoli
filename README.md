# TRANSPOLI

## Computador de Bordo para Euro Truck Simulator 2

Aplicativo desktop Windows desenvolvido em **C# + .NET 10 + WPF**, com identidade própria da Transpoli.

**Desenvolvedor:** Felipe Andrade

### Estado atual — v0.1.0

- Dashboard com visual de computador de bordo
- Estrutura preparada para telemetria ETS2
- Indicadores de veículo, combustível, odômetro, viagem e tacógrafo
- Botão de atualização integrado ao GitHub Releases
- Updater separado para instalar novas versões sem reinstalação manual
- GitHub Actions para build Windows e empacotamento ZIP

### Fluxo de atualização

1. O usuário clica em **ATUALIZAR** no Transpoli.
2. O aplicativo consulta a última release no GitHub.
3. Se houver versão mais nova, o usuário confirma.
4. O Transpoli fecha.
5. O **Transpoli.Updater** baixa o pacote, substitui os arquivos e abre novamente o aplicativo.

### Releases

A pipeline em `.github/workflows/build-release.yml` compila o projeto no Windows e gera `Transpoli-win-x64.zip`. Tags no formato `v*` publicam automaticamente uma GitHub Release com esse pacote.

### Próximas etapas

- Integração real com SCS Telemetry SDK
- Tela de viagem e ETA
- Tacógrafo real
- Carga e integridade
- Combustível e abastecimentos
- Meu Banco
- Histórico e estatísticas
- Configurações
- Assets oficiais da marca