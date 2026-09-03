using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServidor
{
    internal class Program
    {
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