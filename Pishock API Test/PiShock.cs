using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Net;
using System.Runtime.InteropServices;
using Refit;
using System.Collections.Concurrent;
using System.Threading;


namespace Pishock_API_Test
{

    //Weird Stuff (no clue how this stuff does)
    [Headers("Content-Type: application/json")]
    internal interface IPiShockApi
    {
        [Post("/")]
        public Task<ApiResponse<string>> SendPiShockCommandAsync([Body] PiRequest request);
    }

    class PiShock
    {
        private string username { get; set; }
        private string apikey { get; set; }
        private string name { get; set; }
        private string url { get; set; }
        private string code { get; set; }

        private readonly IPiShockApi _piShockApi;
        private PiRequest request { get; set; }
        // Queue stuff (no clue how any of this stuff works)
        private ConcurrentQueue<Func<Task<int>>> commandQueue;
        private SemaphoreSlim queueSemaphore;
        private CancellationTokenSource queueCancellationTokenSource;


        public PiShock(string username, string apikey, string code)
        {
            request = new PiRequest
            {
                Username = username,
                Name = "UwUBot",
                Code = code,
                Apikey = apikey,
            };
            _piShockApi = RestService.For<IPiShockApi>("https://do.pishock.com/api/apioperate");

            commandQueue = new ConcurrentQueue<Func<Task<int>>>();
            queueSemaphore = new SemaphoreSlim(1);
            queueCancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => ProcessCommandQueueAsync());

        }
        public async Task shock(int intensity, int duration)
        {
            Console.WriteLine("Addig shock to queue");

            await AddToCommandQueue(async () => await shockinternal(intensity, duration));
        }
        public async Task<int> shockinternal(int intensity, int duration)
        {
            if (intensity < 1 || intensity > 100) intensity = 25;
            if (duration < 1 || duration > 15) duration = 1;
            request.Duration = duration;
            request.Intensity = intensity;
            request.Op = 0;
            ApiResponse<string> response = await _piShockApi.SendPiShockCommandAsync(request).ConfigureAwait(false);
            Console.WriteLine(response.Content);
            return duration;
        }
        public async Task vibe(int intensity, int duration)
        {
            Console.WriteLine("Addig vibe to queue");

            await AddToCommandQueue(async () => await vibeinternal(intensity, duration));
        }

        public async Task<int> vibeinternal(int intensity, int duration)
        {
            if (intensity < 1 || intensity > 100) intensity = 25;
            if (duration < 1 || duration > 15) duration = 1;
            request.Duration = duration;
            request.Intensity = intensity;
            request.Op = 1;
            ApiResponse<string> response = await _piShockApi.SendPiShockCommandAsync(request).ConfigureAwait(false);
            Console.WriteLine(response.Content);
            return duration;
        }
        public async Task beep(int duration)
        {
            Console.WriteLine("Addig beep to queue");

            await AddToCommandQueue(async () => await beepinternal(duration));
        }

        public async Task<int> beepinternal(int duration)
        {
            if (duration < 1 || duration > 15) duration = 1;
            request.Duration = duration;
            request.Op = 2;
            ApiResponse<string> response = await _piShockApi.SendPiShockCommandAsync(request).ConfigureAwait(false);
            Console.WriteLine(response.Content);
            return duration;
        }
        

        private async Task AddToCommandQueue(Func<Task<int>> command)
        {
            commandQueue.Enqueue(command); 
            await queueSemaphore.WaitAsync();
        }

        private async Task ProcessCommandQueueAsync()
        {
            while (!queueCancellationTokenSource.Token.IsCancellationRequested)
            {
                Func<Task<int>> command;
                if (commandQueue.TryDequeue(out command))
                {
                    Console.WriteLine("Trying to execute:");
                    Console.WriteLine(command.ToString());
                    try
                    {
                        int durationwait = await command.Invoke();
                        Thread.Sleep(durationwait*1000+1000);
                        queueSemaphore.Release();
                        Console.WriteLine("Finished executing");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error occurred while processing command: {ex.Message}");
                        queueSemaphore.Release();
                    }
                }
                else
                {
                    await Task.Delay(500);
                }
            }
        }
    }

    class PiRequest
    {
        public string Username { get; set; }
        public string Apikey { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Op { get; set; }
        public int Duration { get; set; }
        public int Intensity { get; set; }
    }


}
