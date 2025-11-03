using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CargoThrive.Infrastructure.Services
{
    public class TimedTaskService : BackgroundService
    {
        private readonly ILogger<TimedTaskService> _logger;

        public TimedTaskService(ILogger<TimedTaskService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 每隔 1 分钟执行一次
            while (!stoppingToken.IsCancellationRequested)
            {
                // 执行定时任务逻辑
                _logger.LogInformation("定时任务正在执行: {Time}", DateTimeOffset.Now);

                // 模拟一个耗时的任务（例如，更新数据库、清理缓存等）
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
