using CargoThrive.Core.Models;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace CargoThrive.Infrastructure.Helpers;

/// <summary>
/// Redis 7.x 操作辅助类（封装常用命令）
/// </summary>
public class RedisHelper : IDisposable
{
    private readonly IDatabase _db;
    private readonly ConnectionMultiplexer _redis;
    private bool _disposed;

    /// <summary>
    /// 构造函数（通过配置初始化Redis连接）
    /// </summary>
    public RedisHelper(IOptions<RedisSettings> redisSettings)
    {
        var settings = redisSettings.Value;
        // 初始化连接（ConnectionMultiplexer是线程安全的，建议全局单例）
        _redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { settings.ConnectionString },
            DefaultDatabase = settings.DefaultDatabase,
            ConnectTimeout = settings.ConnectTimeout,
            SyncTimeout = settings.SyncTimeout,
            AbortOnConnectFail = false // 连接失败不抛出异常，后续操作会重试
        });

        // 获取数据库实例
        _db = _redis.GetDatabase();
    }

    #region 字符串操作（String）

    /// <summary>
    /// 设置字符串键值对（带过期时间）
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">值（自动序列化为JSON）</param>
    /// <param name="expiry">过期时间（null表示永久）</param>
    public async Task SetStringAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        var json = SerializeToJson(value);
        await _db.StringSetAsync(key, json, expiry);
    }

    /// <summary>
    /// 获取字符串值
    /// </summary>
    public async Task<T?> GetStringAsync<T>(string key)
    {
        var json = await _db.StringGetAsync(key);
        return json.IsNullOrEmpty ? default : DeserializeFromJson<T>(json);
    }

    /// <summary>
    /// 删除键
    /// </summary>
    public async Task<bool> DeleteKeyAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }

    /// <summary>
    /// 检查键是否存在
    /// </summary>
    public async Task<bool> KeyExistsAsync(string key)
    {
        return await _db.KeyExistsAsync(key);
    }

    /// <summary>
    /// 设置键过期时间
    /// </summary>
    public async Task<bool> SetExpiryAsync(string key, TimeSpan expiry)
    {
        return await _db.KeyExpireAsync(key, expiry);
    }

    #endregion

    #region 哈希操作（Hash）

    /// <summary>
    /// 向哈希表添加字段
    /// </summary>
    public async Task HashSetAsync<T>(string key, string field, T value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        var json = SerializeToJson(value);
        await _db.HashSetAsync(key, field, json);
    }

    /// <summary>
    /// 获取哈希表字段值
    /// </summary>
    public async Task<T?> HashGetAsync<T>(string key, string field)
    {
        var json = await _db.HashGetAsync(key, field);
        return json.IsNullOrEmpty ? default : DeserializeFromJson<T>(json);
    }

    /// <summary>
    /// 获取哈希表所有字段和值
    /// </summary>
    public async Task<Dictionary<string, T?>> HashGetAllAsync<T>(string key)
    {
        var hashEntries = await _db.HashGetAllAsync(key);

        // 使用 .ToDictionary 将哈希表转换为 Dictionary<string, T?>
        return hashEntries.ToDictionary(
            entry => entry.Name.ToString(), // 显式将 Name 转换为 string
            entry => entry.Value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(entry.Value) // 反序列化
        );
    }


    /// <summary>
    /// 删除哈希表字段
    /// </summary>
    public async Task<bool> HashDeleteAsync(string key, string field)
    {
        return await _db.HashDeleteAsync(key, field);
    }

    #endregion

    #region 列表操作（List）

    /// <summary>
    /// 向列表左侧添加元素
    /// </summary>
    public async Task<long> ListLeftPushAsync<T>(string key, T value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        var json = SerializeToJson(value);
        return await _db.ListLeftPushAsync(key, json);
    }

    /// <summary>
    /// 向列表右侧添加元素
    /// </summary>
    public async Task<long> ListRightPushAsync<T>(string key, T value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        var json = SerializeToJson(value);
        return await _db.ListRightPushAsync(key, json);
    }

    /// <summary>
    /// 获取列表范围内的元素
    /// </summary>
    public async Task<List<T?>> ListRangeAsync<T>(string key, long start = 0, long stop = -1)
    {
        var values = await _db.ListRangeAsync(key, start, stop);
        return values.Select(v => v.IsNullOrEmpty ? default : DeserializeFromJson<T>(v)).ToList();
    }

    /// <summary>
    /// 移除并返回列表左侧第一个元素
    /// </summary>
    public async Task<T?> ListLeftPopAsync<T>(string key)
    {
        var json = await _db.ListLeftPopAsync(key);
        return json.IsNullOrEmpty ? default : DeserializeFromJson<T>(json);
    }

    #endregion

    #region 集合操作（Set）

    /// <summary>
    /// 向集合添加元素
    /// </summary>
    public async Task<bool> SetAddAsync<T>(string key, T value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        var json = SerializeToJson(value);
        return await _db.SetAddAsync(key, json);
    }

    /// <summary>
    /// 获取集合所有元素
    /// </summary>
    public async Task<List<T?>> SetMembersAsync<T>(string key)
    {
        var values = await _db.SetMembersAsync(key);
        return values.Select(v => v.IsNullOrEmpty ? default : DeserializeFromJson<T>(v)).ToList();
    }

    /// <summary>
    /// 检查元素是否在集合中
    /// </summary>
    public async Task<bool> SetContainsAsync<T>(string key, T value)
    {
        if (value == null) return false;
        var json = SerializeToJson(value);
        return await _db.SetContainsAsync(key, json);
    }

    /// <summary>
    /// 从集合移除元素
    /// </summary>
    public async Task<bool> SetRemoveAsync<T>(string key, T value)
    {
        if (value == null) return false;
        var json = SerializeToJson(value);
        return await _db.SetRemoveAsync(key, json);
    }

    #endregion

    #region 分布式锁（基于Redis实现）

    /// <summary>
    /// 获取分布式锁
    /// </summary>
    public async Task<bool> AcquireLockAsync(string lockKey, string lockValue, TimeSpan expiry)
    {
        return await _db.StringSetAsync(lockKey, lockValue, expiry, When.NotExists);
    }

    /// <summary>
    /// 释放分布式锁（Lua脚本保证原子性）
    /// </summary>
    public async Task<bool> ReleaseLockAsync(string lockKey, string lockValue)
    {
        const string script = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end";
        var result = await _db.ScriptEvaluateAsync(script, new RedisKey[] { lockKey }, new RedisValue[] { lockValue });
        return (long)result == 1;
    }

    #endregion

    #region 辅助方法

    private string SerializeToJson<T>(T value)
    {
        return JsonSerializer.Serialize(value);
    }

    private T? DeserializeFromJson<T>(RedisValue value)
    {
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value);
    }

    #endregion

    #region 释放资源

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _redis?.Dispose();
        }
        _disposed = true;
    }

    ~RedisHelper()
    {
        Dispose(false);
    }

    #endregion
}
