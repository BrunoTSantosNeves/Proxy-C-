# Documentação do Projeto ProxyFallbackAPI

O **ProxyFallbackAPI** é uma aplicação que demonstra boas práticas de design de software para lidar com problemas de alta disponibilidade em APIs, utilizando fallback, proteção contra ataques DDoS e controle de taxa de requisições (Rate Limiting). Além disso, implementa autenticação baseada em tokens para maior segurança. Este documento detalha cada componente do projeto de forma clara e objetiva.

## Lógica de fallback e controle de erros
A lógica de fallback está implementada no **ProxyController.cs**, que age como ponto de entrada para as requisições enviadas à API principal. Caso a API principal falhe, o controlador tenta uma API secundária configurada. Este comportamento garante que o sistema continue funcionando, mesmo em cenários de indisponibilidade parcial.

# Estrutura de Arquivos do Projeto
``````
ProxyFallbackAPI
├── Controllers
│   ├── AuthController.cs           # Gerencia autenticação, login e emissão de tokens JWT.
│   ├── ProxyController.cs          # Processa requisições ao proxy e gerencia o fallback para APIs secundárias.
├── Security
│   ├── Configurations
│   │   └── JwtConfigurations.cs    # Configuração para autenticação JWT, incluindo chave secreta e tempos de expiração.
│   ├── Middleware
│   │   ├── AntiDdosMiddleware.cs   # Implementa proteção contra ataques DDoS e rate limiting.
│   │   └── AuthenticationMiddleware.cs # Middleware para verificar autenticação JWT em requisições.
├── Services
│   ├── IServices.cs                # Define as interfaces para serviços utilizados pelo projeto.
│   ├── IUserservices.cs            # Interface para manipulação de usuários (ex.: validação).
│   ├── TokenService.cs             # Implementa lógica para criação e validação de tokens JWT.
├── Models
│   └── (Adicionar classes de modelo, se necessário.)
├── wwwroot
│   └── bloqueado.html              # Página HTML exibida quando um IP é bloqueado pelo middleware AntiDDoS.
├── appsettings.Development.json    # Configurações para o ambiente de desenvolvimento.
├── appsettings.json                # Configurações gerais, incluindo chaves para JWT.
├── Program.cs                      # Ponto de entrada do aplicativo; configura serviços e middlewares.
├── ProxyFallbackAPI.csproj         # Arquivo de projeto do .NET.
├── README.md                       # Documentação do projeto.
``````

O controlador utiliza serviços definidos no arquivo **ITservices.cs** para gerenciar chamadas às APIs. Esse serviço abstrai a lógica de fallback, tornando o código do controlador mais limpo e fácil de manter.

## Middleware de segurança
O middleware de segurança é responsável por proteger a aplicação contra acessos indevidos e atividades maliciosas. Existem dois middlewares principais:

1. **AntiDdosMiddleware.cs**: Este arquivo implementa um mecanismo de Rate Limiting para evitar que um único IP faça muitas requisições em um curto intervalo de tempo, protegendo contra ataques DDoS. A lógica usa um cache em memória para monitorar o número de requisições por IP. Se o limite configurado for ultrapassado, o IP é bloqueado temporariamente e redirecionado para a página de bloqueio (bloqueado.html). 

   - **Verificação de IP bloqueado**: Se o IP estiver na lista de bloqueados, uma resposta 302 é enviada com um redirecionamento para uma página de erro estática.
   - **Controle de requisições**: Cada requisição é registrada em cache e, se o limite for atingido, o IP é bloqueado por um tempo pré-definido.

2. **AuthenticationMiddleware.cs**: Este middleware valida se as requisições incluem um token de autenticação válido. Ele verifica os cabeçalhos HTTP para garantir que somente usuários autorizados possam acessar os endpoints protegidos. Tokens inválidos ou ausentes resultam em uma resposta 401 Unauthorized.

## Serviços e lógica de negócios
Os serviços contêm a lógica de negócios do projeto e são definidos na pasta **Services**. 

- **ITservices.cs** e **IUserServices.cs**: Arquivos que definem as interfaces para os serviços de API e de usuário, respectivamente. Eles promovem a abstração e permitem a implementação de diferentes estratégias para cada serviço.
- **TokenService.cs**: Este arquivo é responsável por gerar e validar tokens JWT. A geração de tokens inclui informações como identificadores de usuário e prazos de validade, enquanto a validação garante que os tokens são autênticos e ainda válidos.

## Controladores
Os controladores expõem os endpoints que podem ser consumidos por clientes externos:

- **ProxyController.cs**: Controla o fluxo de requisições entre os clientes e as APIs externas. Implementa a lógica de fallback caso a API principal falhe.
- **AuthController.cs**: Gerencia a autenticação e fornece endpoints para login, geração e renovação de tokens.

## Configurações
As configurações específicas do projeto estão armazenadas nos arquivos JSON:

- **appsettings.json** e **appsettings.Development.json**: Contêm configurações como strings de conexão, URLs das APIs principal e secundária, e parâmetros para o middleware (como limites de requisição e tempo de bloqueio).

## Página de erro estática
A página **bloqueado.html**, localizada na pasta **wwwroot**, é uma página HTML simples exibida quando um IP é bloqueado pelo middleware de proteção contra DDoS. Ela informa ao usuário que seu acesso foi negado devido a atividades suspeitas. Essa página pode ser personalizada para melhorar a experiência do usuário.

## Testes
Os testes estão localizados na pasta **Tests**. Eles garantem que a lógica de fallback, autenticação e segurança funcionem conforme o esperado. A cobertura de testes pode ser visualizada através dos relatórios armazenados na pasta **coverage-report**.

## Estrutura geral do projeto
O projeto é organizado de forma modular para facilitar a manutenção e a escalabilidade. Cada funcionalidade principal está isolada em arquivos ou pastas específicas, promovendo o princípio de responsabilidade única.

Com essa documentação, espera-se que o projeto ProxyFallbackAPI seja compreendido facilmente e possa servir de base para estudos e extensões futuras. Caso haja dúvidas ou melhorias a serem feitas, o projeto está aberto para contribuições.

