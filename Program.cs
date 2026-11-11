using System;
using System.Collections.Generic;
using System.Linq;

public enum Estado { READY, RUNNING, TERMINATED }

public class PCB
{
    public int Pid, Prioridad, Llegada, Rafaga, Restante, Espera, Retorno;
    public string Nombre = "";
    public Estado Estado = Estado.READY;
}

public class Program
{
    static List<PCB> Copiar(List<PCB> originales)
    {
        return originales.Select(p => new PCB
        {
            Pid = p.Pid, Nombre = p.Nombre, Prioridad = p.Prioridad,
            Llegada = p.Llegada, Rafaga = p.Rafaga, Restante = p.Rafaga
        }).ToList();
    }

    static double[] Mostrar(string algoritmo, List<PCB> procesos, List<string> gantt)
    {
        Console.WriteLine("\n--- " + algoritmo + " ---");
        Console.WriteLine("Gantt: " + string.Join(" -> ", gantt));
        Console.WriteLine("PID  Nombre    Espera  Retorno");
        foreach (PCB p in procesos.OrderBy(p => p.Pid))
            Console.WriteLine($"{p.Pid,-4} {p.Nombre,-9} {p.Espera,-7} {p.Retorno}");
        Console.WriteLine($"Espera promedio: {procesos.Average(p => p.Espera):F2}");
        Console.WriteLine($"Retorno promedio: {procesos.Average(p => p.Retorno):F2}");
        return new double[] { procesos.Average(p => p.Espera), procesos.Average(p => p.Retorno) };
    }

    static double[] FCFS(List<PCB> procesos)
    {
        Queue<PCB> ready = new Queue<PCB>(procesos.OrderBy(p => p.Llegada));
        List<string> gantt = new List<string>();
        int tiempo = 0;

        while (ready.Count > 0)
        {
            PCB p = ready.Dequeue();
            if (tiempo < p.Llegada)
            {
                gantt.Add($"Libre ({tiempo}-{p.Llegada})");
                tiempo = p.Llegada;
            }
            p.Estado = Estado.RUNNING;
            gantt.Add($"{p.Nombre} ({tiempo}-{tiempo + p.Rafaga})");
            tiempo += p.Rafaga;
            p.Retorno = tiempo - p.Llegada;
            p.Espera = p.Retorno - p.Rafaga;
            p.Estado = Estado.TERMINATED;
        }
        return Mostrar("FCFS", procesos, gantt);
    }

    static double[] RoundRobin(List<PCB> procesos, int quantum)
    {
        procesos = procesos.OrderBy(p => p.Llegada).ToList();
        Queue<PCB> ready = new Queue<PCB>();
        List<string> gantt = new List<string>();
        int tiempo = 0, siguiente = 0, terminados = 0;

        while (terminados < procesos.Count)
        {
            while (siguiente < procesos.Count && procesos[siguiente].Llegada <= tiempo)
                ready.Enqueue(procesos[siguiente++]);

            if (ready.Count == 0)
            {
                int llegada = procesos[siguiente].Llegada;
                gantt.Add($"Libre ({tiempo}-{llegada})");
                tiempo = llegada;
                continue;
            }

            PCB p = ready.Dequeue();
            p.Estado = Estado.RUNNING;
            int duracion = Math.Min(quantum, p.Restante);
            gantt.Add($"{p.Nombre} ({tiempo}-{tiempo + duracion})");
            tiempo += duracion;
            p.Restante -= duracion;

            // Los procesos que llegaron durante el turno entran a READY.
            while (siguiente < procesos.Count && procesos[siguiente].Llegada <= tiempo)
                ready.Enqueue(procesos[siguiente++]);

            if (p.Restante > 0)
            {
                p.Estado = Estado.READY;
                ready.Enqueue(p);
            }
            else
            {
                p.Estado = Estado.TERMINATED;
                p.Retorno = tiempo - p.Llegada;
                p.Espera = p.Retorno - p.Rafaga;
                terminados++;
            }
        }
        return Mostrar("Round Robin (quantum " + quantum + ")", procesos, gantt);
    }

    public static void Main()
    {
        List<PCB> procesos = new List<PCB>
        {
            new PCB { Pid = 1, Nombre = "Backup", Prioridad = 2, Llegada = 0, Rafaga = 5 },
            new PCB { Pid = 2, Nombre = "Logs", Prioridad = 1, Llegada = 1, Rafaga = 3 },
            new PCB { Pid = 3, Nombre = "Reporte", Prioridad = 3, Llegada = 2, Rafaga = 8 }
        };

        Console.WriteLine("SIMULADOR DE TAREAS EN NUBE PRIVADA");
        Console.Write("Quantum de Round Robin (ejemplo: 2): ");
        if (!int.TryParse(Console.ReadLine(), out int quantum) || quantum <= 0)
        {
            Console.WriteLine("Quantum invalido, se usara 2.");
            quantum = 2;
        }

        double[] fcfs = FCFS(Copiar(procesos));
        double[] rr = RoundRobin(Copiar(procesos), quantum);
        Console.WriteLine("\nCOMPARACION FINAL");
        Console.WriteLine("Algoritmo       Espera media   Retorno medio");
        Console.WriteLine($"FCFS            {fcfs[0],-14:F2} {fcfs[1]:F2}");
        Console.WriteLine($"Round Robin     {rr[0],-14:F2} {rr[1]:F2}");
    }
}
