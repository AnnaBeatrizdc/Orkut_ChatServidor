using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;

namespace ChatServidor
{
    internal class Program
    {
        static void EnviarListaUsuarios(Socket servidor, Dictionary<string, EndPoint> clientes, Dictionary<string, string> nomesClientes)
        {
            string listaUsuarios = "USUARIOS";

            foreach (string nome in nomesClientes.Values)
            {
                listaUsuarios += "|" + nome;
            }

            byte[] dados = Encoding.UTF8.GetBytes(listaUsuarios);

            foreach (EndPoint cliente in clientes.Values)
            {
                servidor.SendTo(dados, cliente);
            }
        }

        static void Main(string[] args)
        {
            int porta = 9060;

            Socket servidor = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp
            );

            IPEndPoint endereco = new IPEndPoint(
                IPAddress.Any,
                porta
            );

            servidor.Bind(endereco);

            // Lista de clientes conectados
            Dictionary<string, EndPoint> clientes =
                new Dictionary<string, EndPoint>();

            Dictionary<string, string> nomesClientes =
                new Dictionary<string, string>();

            Console.WriteLine("=================================");
            Console.WriteLine("       SERVIDOR DE CHAT");
            Console.WriteLine("=================================");
            Console.WriteLine();
            Console.WriteLine($"Servidor iniciado na porta {porta}");
            Console.WriteLine("Aguardando mensagens...");
            Console.WriteLine();

            while (true)
            {
                byte[] dados = new byte[1024];

                EndPoint remetente = new IPEndPoint(
                    IPAddress.Any,
                    0
                );

                int quantidade = servidor.ReceiveFrom(
                    dados,
                    ref remetente
                );

                string mensagem = Encoding.UTF8.GetString(
                    dados,
                    0,
                    quantidade
                );

                string enderecoCliente = remetente.ToString();

                if (mensagem == "PING")
                {
                    byte[] resposta = Encoding.UTF8.GetBytes("PONG");

                    servidor.SendTo(resposta, remetente);

                    continue;
                }

                if (mensagem.StartsWith("VERIFICAR_NOME|"))
                {
                    string[] partes = mensagem.Split('|');

                    if (partes.Length >= 2)
                    {
                        string nome = partes[1];

                        bool nomeJaExiste = nomesClientes.Values.Any(
                            nomeExistente =>
                                nomeExistente.Equals(
                                    nome,
                                    StringComparison.OrdinalIgnoreCase
                                )
                        );

                        string resposta;

                        if (nomeJaExiste)
                        {
                            resposta = "NOME_OCUPADO";
                        }
                        else
                        {
                            resposta = "NOME_DISPONIVEL";
                        }

                        byte[] dadosResposta =
                            Encoding.UTF8.GetBytes(resposta);

                        servidor.SendTo(
                            dadosResposta,
                            remetente
                        );
                    }

                    continue;
                }

                if (mensagem.StartsWith("CONECTAR|"))
                {
                    string[] partes = mensagem.Split('|');

                    if (partes.Length >= 2)
                    {
                        string nome = partes[1];
                        bool nomeJaExiste = nomesClientes.Values.Any(nomeExistente =>nomeExistente.Equals(nome,StringComparison.OrdinalIgnoreCase));

                        if (nomeJaExiste)
                        {
                            string mensagemErro = "ERRO|NOME_EM_USO";

                            byte[] dadosErro =
                                Encoding.UTF8.GetBytes(mensagemErro);

                            servidor.SendTo(
                                dadosErro,
                                remetente
                            );

                            Console.WriteLine(
                                $"Tentativa de conexão recusada: nome '{nome}' já está em uso."
                            );

                            continue;
                        }

                        if (!clientes.ContainsKey(enderecoCliente))
                        {
                            clientes.Add(enderecoCliente, remetente);
                        }

                        nomesClientes[enderecoCliente] = nome;

                        Console.WriteLine(
                            $"Usuário conectado: {nome} - {enderecoCliente}"
                        );

                        Console.WriteLine(
                            $"Total de clientes: {clientes.Count}"
                        );

                        // Envia a lista atualizada para todos os clientes
                        EnviarListaUsuarios(
                            servidor,
                            clientes,
                            nomesClientes
                        );
                    }

                    continue;
                }

                if (mensagem.StartsWith("DESCONECTAR|"))
                {
                    string[] partes = mensagem.Split('|');

                    if (partes.Length >= 2)
                    {
                        string nome = partes[1];

                        if (clientes.ContainsKey(enderecoCliente))
                        {
                            clientes.Remove(enderecoCliente);
                        }

                        if (nomesClientes.ContainsKey(enderecoCliente))
                        {
                            nomesClientes.Remove(enderecoCliente);
                        }

                        Console.WriteLine(
                            $"Usuário desconectado: {nome}"
                        );

                        Console.WriteLine(
                            $"Total de clientes: {clientes.Count}"
                        );

                        EnviarListaUsuarios(
                            servidor,
                            clientes,
                            nomesClientes
                        );
                    }

                    continue;
                }

                if (mensagem.StartsWith("GERAL|"))
                {
                    string[] partes = mensagem.Split('|', 2);

                    if (partes.Length == 2)
                    {
                        string texto = partes[1];

                        string remetenteNome = nomesClientes[enderecoCliente];

                        string mensagemEnviar =
                            "GERAL|" + remetenteNome + "|" + texto;

                        byte[] resposta = Encoding.UTF8.GetBytes(
                            mensagemEnviar
                        );

                        foreach (EndPoint cliente in clientes.Values)
                        {
                            servidor.SendTo(
                                resposta,
                                cliente
                            );
                        }

                        Console.WriteLine(
                            $"[GERAL] {remetenteNome}: {texto}"
                        );
                    }

                    continue;
                }

                if (mensagem.StartsWith("MENSAGEM|"))
                {
                    string[] partes = mensagem.Split('|', 3);

                    if (partes.Length == 3)
                    {
                        string destinatario = partes[1];
                        string texto = partes[2];

                        string remetenteNome = nomesClientes[enderecoCliente];

                        foreach (var cliente in nomesClientes)
                        {
                            if (cliente.Value == destinatario)
                            {
                                EndPoint enderecoDestinatario = clientes[cliente.Key];

                                string mensagemEnviar =
                                    "MENSAGEM|" + remetenteNome + "|" + texto;

                                byte[] resposta = Encoding.UTF8.GetBytes(
                                    mensagemEnviar
                                );

                                servidor.SendTo(
                                    resposta,
                                    enderecoDestinatario
                                );

                                Console.WriteLine(
                                    $"{remetenteNome} -> {destinatario}: {texto}"
                                );

                                break;
                            }
                        }
                    }

                    continue;
                }
            }
        }
    }
}