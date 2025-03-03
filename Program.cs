using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("Procesando pedidos...");

        // Simulación de múltiples pedidos
        var pedidos = new List<Task>
        {
            ProcesarPedido(1),
            ProcesarPedido(2),
            ProcesarPedido(3),
            ProcesarPedido(4),
            ProcesarPedido(5)
        };

        // Esperar a que el primer pedido termine para continuar con otras tareas
        await Task.WhenAny(pedidos);

        Console.WriteLine("Al menos un pedido ha sido completado.");
        await Task.WhenAll(pedidos);
        Console.WriteLine("Todos los pedidos han sido procesados.");
    }

    static async Task ProcesarPedido(int pedidoId)
    {
        Console.WriteLine($"Pedido {pedidoId}: Iniciando procesamiento...");

        try
        {
            // Usando Task.Factory.StartNew y TaskCreationOptions.AttachedToParent
            var validacion = Task.Factory.StartNew(async () =>
            {
                await Task.Delay(1000);
                Console.WriteLine($"Pedido {pedidoId}: Validado.");
            }, TaskCreationOptions.AttachedToParent);

            // Usando Task.Run para el procesamiento del pago
            var procesamientoPago = Task.Run(async () =>
            {
                await Task.Delay(2000);
                if (new Random().Next(0, 5) == 0)
                    throw new Exception("Pago rechazado.");
                Console.WriteLine($"Pedido {pedidoId}: Pago procesado.");
            });

            // Usando ContinueWith con TaskContinuationOptions.OnlyOnRanToCompletion
            var factura = procesamientoPago.ContinueWith(async t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    await Task.Delay(1000);
                    Console.WriteLine($"Pedido {pedidoId}: Factura generada y pedido confirmado.");
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion).Unwrap();

            // Manejo de error usando TaskContinuationOptions.OnlyOnFaulted
            procesamientoPago.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine($"Pedido {pedidoId}: ERROR - {t.Exception?.InnerException?.Message}");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);

            // Manejo de cancelación usando TaskContinuationOptions.OnlyOnCanceled
            procesamientoPago.ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    Console.WriteLine($"Pedido {pedidoId}: ERROR - Tarea cancelada.");
                }
            }, TaskContinuationOptions.OnlyOnCanceled);

            // Esperar la validación y el pago
            await validacion;
            await procesamientoPago;
            await factura;

            Console.WriteLine($"Pedido {pedidoId}: Finalizado.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Pedido {pedidoId}: Error inesperado - {ex.Message}");
        }
    }
}
