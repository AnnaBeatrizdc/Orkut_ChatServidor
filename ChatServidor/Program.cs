using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

                if (mensagem.StartsWith("CONECTAR|"))
                {
                    string[] partes = mensagem.Split('|');

                    if (partes.Length >= 2)
                    {
                        string nome = partes[1];

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

                // Verifica se o cliente já está registrado
                if (!clientes.ContainsKey(enderecoCliente))
                {
                    clientes.Add(enderecoCliente, remetente);

                    Console.WriteLine(
                        $"Novo cliente conectado: {enderecoCliente}"
                    );

                    Console.WriteLine(
                        $"Total de clientes: {clientes.Count}"
                    );
                }

                Console.WriteLine(
                    $"{enderecoCliente}: {mensagem}"
                );

                // Envia a mensagem para todos os clientes
                foreach (EndPoint cliente in clientes.Values)
                {
                    string mensagemEnviar =
                        $"{enderecoCliente}: {mensagem}";

                    byte[] resposta = Encoding.UTF8.GetBytes(
                        mensagemEnviar
                    );

                    servidor.SendTo(
                        resposta,
                        cliente
                    );
                }
            }
        }
    }
}