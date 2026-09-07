# 🖥️ Chat Servidor

Servidor desenvolvido em **C#**, responsável por gerenciar a comunicação entre os usuários do sistema de chat através de **Sockets UDP**.

O servidor centraliza as conexões, mantém o controle dos usuários conectados e encaminha mensagens privadas ou gerais para os clientes correspondentes.

---

## ✨ Funcionalidades

- Gerenciamento de múltiplos clientes
- Registro de usuários conectados
- Validação de nomes duplicados
- Atualização da lista de usuários online
- Encaminhamento de mensagens privadas
- Distribuição de mensagens no Chat Geral
- Identificação de desconexões
- Comunicação através de UDP
- Resposta ao monitoramento `PING / PONG`
- Suporte à reconexão dos clientes
- Tratamento de nomes sem diferenciar letras maiúsculas e minúsculas

---

## 🛠️ Tecnologias

- C#
- .NET
- Sockets
- UDP
- Git e GitHub

---

## 🏗️ Arquitetura

O servidor funciona como intermediário entre todos os clientes.

```text
ChatCliente
     │
     │ UDP
     ▼
┌─────────────────┐
│  ChatServidor   │
│   Porta 9060    │
└─────────────────┘
     ▲
     │ UDP
     │
ChatCliente
```

Nas **conversas privadas**, o servidor identifica o destinatário e encaminha a mensagem somente para ele.

No **Chat Geral**, a mensagem é distribuída para todos os usuários conectados.

---

## 🔄 Comunicação

A comunicação entre cliente e servidor utiliza mensagens de texto seguindo um protocolo simples.

| Comando | Função |
|---|---|
| `PING` | Verifica a disponibilidade do servidor |
| `CONECTAR` | Registra um novo usuário |
| `DESCONECTAR` | Remove um usuário conectado |
| `VERIFICAR_NOME` | Verifica se um nome já está em uso |
| `USUARIOS` | Envia a lista atualizada de usuários |
| `MENSAGEM` | Encaminha mensagens privadas |
| `GERAL` | Distribui mensagens para todos |

---

## ▶️ Como executar

1. Abra o projeto **ChatServidor**.
2. Execute a aplicação.
3. O servidor ficará aguardando conexões UDP na porta `9060`.
4. Execute uma ou mais instâncias do **ChatCliente**.
5. Os usuários conectados serão exibidos no console do servidor.

> Os clientes devem utilizar o endereço IP da máquina onde o servidor está sendo executado.

---

## 💬 Chat Cliente

Este projeto funciona em conjunto com o **ChatCliente**, responsável pela interface gráfica e interação dos usuários.

O servidor deve ser iniciado antes dos clientes para permitir a conexão.

---

## 🚀 Próxima etapa

A versão atual utiliza **Sockets UDP em rede local**.

Na próxima fase, o servidor será adaptado para comunicação através da internet utilizando **ASP.NET Core e SignalR**.

---

## 👩‍💻 Desenvolvedores

**Ana Beatriz**  
**Eduardo Paiva**

Projeto acadêmico desenvolvido em C# utilizando arquitetura Cliente/Servidor.
