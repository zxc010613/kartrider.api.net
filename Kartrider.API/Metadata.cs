﻿using Kartrider.API.Model;
using Kartrider.API.Json.Converter;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace Kartrider.API
{
    /// <summary>
    /// 카트라이더 메타데이터 클래스
    /// </summary>
    public class Metadata
    {

        private Dictionary<string, string> _character = new Dictionary<string, string>();
        private Dictionary<string, string> _flyingPet = new Dictionary<string, string>();
        private Dictionary<string, string> _gameType = new Dictionary<string, string>();
        private Dictionary<string, string> _kart = new Dictionary<string, string>();
        private Dictionary<string, string> _pet = new Dictionary<string, string>();
        private Dictionary<string, string> _track = new Dictionary<string, string>();

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="init">true면 메타데이터 다운로드 후 업데이트</param>
        public Metadata(bool init = false)
        {
            if (init)
            {
                string path = Path.Combine(Path.GetTempPath(), "metadata.zip");
                KartApi.DownloadMetadata(path);
                Update(path, true);
            }
        }
        /// <summary>
        /// 메타데이터에서 키에 해당하는 값을 가져온다.
        /// </summary>
        /// <param name="type">메타데이터 타입</param>
        /// <param name="id">메타데이터 아이디</param>
        /// <param name="defaultValue">해당 메타데이터 아이디가 없는 경우 반환할 값</param>
        /// <returns></returns>
        public string this[MetadataType type, string id, string defaultValue]
        {
            get
            {
                try
                {
                    return this[type, id];
                }
                catch (KeyNotFoundException)
                {
                    return defaultValue;
                }
            }
        }
        /// <summary>
        /// 메타데이터에서 키에 해당하는 값을 가져온다.
        /// </summary>
        /// <param name="type">메타데이터 타입</param>
        /// <param name="id">메타데이터 아이디</param>
        /// <returns>키에 해당하는 값</returns>
        public string this[MetadataType type, string id]
        {
            get
            {
                IDictionary<string, string> dic = null;
                switch (type)
                {
                    case MetadataType.Character:
                        dic = _character;
                        break;
                    case MetadataType.FlyingPet:
                        dic = _flyingPet;
                        break;
                    case MetadataType.GameType:
                        dic = _gameType;
                        break;
                    case MetadataType.Kart:
                        dic = _kart;
                        break;
                    case MetadataType.Pet:
                        dic = _pet;
                        break;
                    case MetadataType.Track:
                        dic = _track;
                        break;

                }
                if (!dic.ContainsKey(id))
                {
                    string keyName = Enum.GetName(typeof(MetadataType), type);
                    throw new KeyNotFoundException($"{id} not found in {keyName}");
                }
                return dic[id];
            }
        }
        /// <summary>
        /// 특정 json파일에 대해서만 업데이트
        /// </summary>
        /// <param name="jsonFile">json 파일</param>
        public void Update(FileInfo jsonFile)
        {
            string fileName = jsonFile.Name;
            string dirPath = jsonFile.DirectoryName;
            if(fileName == "character.json")
            {
                LoadMetadata(dirPath, "character.json", out _character);
            }
            else if(fileName == "flyingPet.json")
            {
                LoadMetadata(dirPath, "flyingPet.json", out _flyingPet);
            }
            else if(fileName == "gameType.json")
            {
                LoadMetadata(dirPath, "gameType.json", out _gameType);
            }
            else if (fileName == "kart.json")
            {
                LoadMetadata(dirPath, "kart.json", out _kart);
            }
            else if (fileName == "pet.json")
            {
                LoadMetadata(dirPath, "pet.json", out _pet);
            }
            else if (fileName == "track.json")
            {
                LoadMetadata(dirPath, "track.json", out _track);
            }
        }
        /// <summary>
        /// 메타데이터 업데이트
        /// </summary>
        /// <param name="filePath">.zip 파일 경로</param>
        /// <param name="fileDelete">업데이트후 .zip 파일을 삭제할 것인지에 대한 여부</param>
        public void Update(string filePath, bool fileDelete = false)
        {
            string tmpPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
            ZipFile.ExtractToDirectory(filePath, tmpPath);
            try
            {
                LoadMetadata(tmpPath, "character.json", out _character);
                LoadMetadata(tmpPath, "flyingPet.json", out _flyingPet);
                LoadMetadata(tmpPath, "gameType.json", out _gameType);
                LoadMetadata(tmpPath, "kart.json", out _kart);
                LoadMetadata(tmpPath, "pet.json", out _pet);
                LoadMetadata(tmpPath, "track.json", out _track);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Directory.Delete(tmpPath, true);
            }
            if (fileDelete)
            {
                File.Delete(filePath);
            }
        }
        /// <summary>
        /// 메타데이터 업데이트
        /// </summary>
        /// <param name="bytes">.zip 파일 데이터가 담긴 byte 배열</param>
        public void Update(byte[] bytes)
        {
            string tmpFilePath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            tmpFilePath = Path.ChangeExtension(tmpFilePath, "zip");
            File.WriteAllBytes(tmpFilePath, bytes);
            Update(tmpFilePath, true);
        }
        /// <summary>
        /// JSON파일을 불러와 Dictionary에 추가한다.
        /// </summary>
        /// <param name="path">JSON파일이 있는 디렉터리 경로</param>
        /// <param name="fileName">JSON 파일 이름(확장자 포함)</param>
        /// <param name="model">JSON 데이터를 삽입할 Dictionary</param>
        private void LoadMetadata(string path, string fileName, out Dictionary<string, string> model)
        {
            string filePath = Path.Combine(path, fileName);
            string content = File.ReadAllText(filePath);
            var deserializeOptions = new JsonSerializerOptions();
            deserializeOptions.Converters.Add(new MetadataConverter());
            model = JsonSerializer.Deserialize<Dictionary<string, string>>(content, deserializeOptions);
        }
        /// <summary>
        /// Hash를 읽을 수 있는 문자열로 변경한다.
        /// </summary>
        /// <param name="matchResponse">Hash를 읽을 수 있는 문자열로 바꿀려는 MatchResponse 클래스</param>
        /// <param name="defaultValue">Hash가 없을 때 사용하는 기본값</param>
        public void HashToString(MatchResponse matchResponse, string defaultValue = null)
        {
            for (int i = 0; i < matchResponse.Matches.Count; i++)
            {
                matchResponse.Matches[i].MatchType = this[MetadataType.GameType, matchResponse.Matches[i].MatchType, defaultValue];
                for (int j = 0; j < matchResponse.Matches[i].Matches.Count; j++)
                {
                    HashToString(matchResponse.Matches[i].Matches[j], defaultValue);
                }
            }
        }
        /// <summary>
        /// Hash를 읽을 수 있는 문자열로 변경한다.
        /// </summary>
        /// <param name="allMatches">Hash를 읽을 수 있는 문자열로 바꿀려는 AllMatches 클래스</param>
        /// <param name="defaultValue">Hash가 없을 때 사용하는 기본값</param>
        public void HashToString(AllMatches allMatches, string defaultValue = null)
        {
            for (int i = 0; i < allMatches.Matches.Count; i++)
            {
                allMatches.Matches[i].MatchType = this[MetadataType.GameType, allMatches.Matches[i].MatchType, defaultValue];
            }
        }
        /// <summary>
        /// Hash를 읽을 수 있는 문자열로 변경한다.
        /// </summary>
        /// <param name="matchDetail">Hash를 읽을 수 있는 문자열로 바꿀려는 MatchDetail 클래스</param>
        /// <param name="defaultValue">Hash가 없을 때 사용하는 기본값</param>
        public void HashToString(MatchDetail matchDetail, string defaultValue = null)
        {
            matchDetail.MatchType = this[MetadataType.GameType, matchDetail.MatchType, defaultValue];
            matchDetail.TrackId = this[MetadataType.Track, matchDetail.TrackId, defaultValue];
            if (matchDetail.MatchResult == TeamId.Solo)
            {
                for (int i = 0; i < matchDetail.Players.Count; i++)
                {
                    HashToString(matchDetail.Players[i], defaultValue);
                }
            }
            else
            {
                for (int i = 0; i < matchDetail.Teams.Count; i++)
                {
                    for (int j = 0; j < matchDetail.Teams[i].Players.Count; j++)
                    {
                        HashToString(matchDetail.Teams[i].Players[j], defaultValue);
                    }
                }
            }
        }
        /// <summary>
        /// Hash를 읽을 수 있는 문자열로 변경한다.
        /// </summary>
        /// <param name="matchInfo">Hash를 읽을 수 있는 문자열로 바꿀려는 MatchInfo 클래스</param>
        /// <param name="defaultValue">Hash가 없을 때 사용하는 기본값</param>
        public void HashToString(MatchInfo matchInfo, string defaultValue = null)
        {
            matchInfo.MatchType = this[MetadataType.GameType, matchInfo.MatchType, defaultValue];
            matchInfo.Character = this[MetadataType.Character, matchInfo.Character, defaultValue];
            matchInfo.TrackId = this[MetadataType.Track, matchInfo.TrackId, defaultValue];
            HashToString(matchInfo.Player, defaultValue);
        }
        /// <summary>
        /// Hash를 읽을 수 있는 문자열로 변경한다.
        /// </summary>
        /// <param name="player">Hash를 읽을 수 있는 문자열로 바꿀려는 Player 클래스</param>
        /// <param name="defaultValue">Hash가 없을 때 사용하는 기본값</param>
        public void HashToString(Player player,string defaultValue = null)
        {
            player.Character = this[MetadataType.Character, player.Character, defaultValue];
            player.Kart = this[MetadataType.Kart, player.Kart, defaultValue];
            player.Pet = this[MetadataType.Pet, player.Pet, defaultValue];
            player.FlyingPet = this[MetadataType.FlyingPet, player.FlyingPet, defaultValue];
        }
        /// <summary>
        /// Hash를 읽을 수 있는 문자열로 변경한다.
        /// </summary>
        /// <param name="match">Hash를 읽을 수 있는 문자열로 바꿀려는 Match 클래스</param>
        /// <param name="defaultValue">Hash가 없을 때 사용하는 기본값</param>
        public void HashToString(Match match,string defaultValue = null)
        {
            match.MatchType = this[MetadataType.GameType, match.MatchType, defaultValue];
            for(int i = 0; i < match.Matches.Count; i++)
            {
                HashToString(match.Matches[i], defaultValue);
            }
        }
        /// <summary>
        /// Hash를 읽을 수 있는 문자열로 변경한다.
        /// </summary>
        /// <param name="team">Hash를 읽을 수 있는 문자열로 바꿀려는 Team 클래스</param>
        /// <param name="defaultValue">Hash가 없을 때 사용하는 기본값</param>
        public void HashToString(Team team, string defaultValue = null)
        {
            for(int i = 0; i < team.Players.Count; i++)
            {
                HashToString(team.Players[i], defaultValue);
            }
        }
        /// <summary>
        /// Hash를 읽을 수 있는 문자열로 변경한다.
        /// </summary>
        /// <param name="matchesByMatchType">Hash를 읽을 수 있는 문자열로 바꿀려는 MatchesByMatchType 클래스</param>
        /// <param name="defaultValue">Hash가 없을 때 사용하는 기본값</param>
        public void HashToString(MatchesByMatchType matchesByMatchType, string defaultValue = null)
        {
            matchesByMatchType.MatchType = this[MetadataType.GameType, matchesByMatchType.MatchType, defaultValue];
        }
    }
}
