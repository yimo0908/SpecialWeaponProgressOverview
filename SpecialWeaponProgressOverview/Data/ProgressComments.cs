using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview.Data;

public static class ProgressComments
{
    private static Dictionary<string, string[]>? _cache;
    private static readonly Random _rng = new();

    private static string JsonPath =>
        Path.Combine(
            PluginService.PluginInterface.AssemblyLocation.DirectoryName!,
            "ProgressComments.json");

    private static Dictionary<string, string[]> Load()
    {
        if (_cache != null)
            return _cache;

        try
        {
            var json = File.ReadAllText(JsonPath);
            _cache = JsonSerializer.Deserialize<Dictionary<string, string[]>>(json);
            return _cache!;
        }
        catch (Exception ex)
        {
            PluginService.PluginLog?.Error($"加载评语文件失败: {ex.Message}");
            _cache = new Dictionary<string, string[]>();
            return _cache;
        }
    }

    public static string GetRandomComment(int progressPercent)
    {
        var data = Load();
        var key = progressPercent switch
        {
            100 => "100",
            >= 80 => "80-99",
            >= 60 => "60-79",
            >= 40 => "40-59",
            >= 20 => "20-39",
            _ => "0-19",
        };

        if (data.TryGetValue(key, out var comments) && comments.Length > 0)
            return comments[_rng.Next(comments.Length)];

        return "";
    }

    public static void Reload()
    {
        _cache = null;
    }
}
