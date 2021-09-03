using Microsoft.AspNetCore.Components;
using System.Text;
using HtmlAgilityPack;
using ProduceWithPandas.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http.Json;

namespace ProduceWithPandas.Pages;

public partial class GuessTheSongName : ComponentBase
{
    private string _actualName;
    private string _actualNameBytes;
    private string _testName;
    private string _testNameBytes;
    private double _cross;
    private double _actualMod;
    private double _testMod;
    private double _result;
    private List<DoubanReply> _records;
    private bool _loading;
    private DateTime _lastRefreshTime;
    [Inject] HttpClient Client { get; set; }

    public GuessTheSongName()
    {
        _actualName = string.Empty;
        _actualNameBytes = string.Empty;
        _testName = string.Empty;
        _testNameBytes = string.Empty;
        _records = new List<DoubanReply>();

        _actualName = "这只是一个十五个字的测试歌曲名";
        _actualNameBytes = string.Join(", ", GetBytesFromString(_actualName).Select(a => a.ToString("X2")));
    }

    protected override Task OnInitializedAsync()
    {

        return base.OnInitializedAsync();
    }

    public void OnSearch()
    {
        if (_testName?.Length != 15)
        {
            return;
        }
        byte[] bytes1 = GetBytesFromString(_actualName);
        byte[] bytes2 = GetBytesFromString(_testName);
        _actualNameBytes = string.Join(", ", bytes1.Select(a => a.ToString("X2")));
        _testNameBytes = string.Join(", ", bytes2.Select(a => a.ToString("X2")));
        _cross = GetCross(bytes1, bytes2);
        _actualMod = GetMod(bytes1);
        _testMod = GetMod(bytes2);
        _result = _cross * 100.0 / _actualMod / _testMod;
    }

    private byte[] GetBytesFromString(string s)
    {
        if (s is null)
        {
            return Array.Empty<byte>();
        }
        return Encoding.Unicode.GetBytes(s);
    }

    private double GetMod(byte[] bytes)
    {
        double result = 0;
        foreach (byte b in bytes)
        {
            result += (1.0 * b * b);
        }
        return Math.Sqrt(result);
    }

    private double GetCross(byte[] bytes1, byte[] bytes2)
    {
        double result = 0;
        int length = Math.Min(bytes1.Length, bytes2.Length);
        for (int i = 0; i < length; i++)
        {
            result += (1.0 * bytes1[i] * bytes2[i]);
        }
        return result;
    }

    private async void OnClick()
    {
        var api = @"https://rabbitism.com/api/api/doubanpost/244506252";
        _loading = true;
        var results= await Client.GetFromJsonAsync<DoubanPostCache>(api);
        
        if (results != null)
        {
            _lastRefreshTime = results.LastUpdateDate;
            _records.Clear();
            Dictionary<int, List<DoubanReply>> _userMap = new Dictionary<int, List<DoubanReply>>();
            results.Replies.OrderBy(a => a.PublishTime);
            foreach(var reply in results.Replies)
            {
                if (!_userMap.ContainsKey(reply.UserId))
                {
                    _userMap[reply.UserId] = new List<DoubanReply>();
                }
                if (_userMap[reply.UserId].Count >= 3)
                {
                    continue;
                }
                _userMap[reply.UserId].Add(reply);
            }

            List<DoubanReply> _replies = new List<DoubanReply>();
            foreach(var value in _userMap.Values)
            {
                _replies.AddRange(value);
            }
            foreach(var record in _records)
            {
                string answer = record.Answer.Trim();
                if(answer != null && answer.StartsWith("歌名是：") && answer.Length>=19)
                {
                    string actualAnswer = answer.Substring(4, 15);
                    byte[] bytes1 = GetBytesFromString(_actualName);
                    byte[] bytes2 = GetBytesFromString(record.Answer);
                    record.Score = 100.0 * GetCross(bytes1, bytes2) / GetMod(bytes1) / GetMod(bytes2);
                    _records.Add(record);
                }
            }
            _records = _records.OrderByDescending(a => a.Score).ToList();
        }

        _loading = false;
        StateHasChanged();
    }
}
