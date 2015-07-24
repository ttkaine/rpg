using System;
using System.Collections;
using System.Collections.Generic;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Abstract
{
    public interface IAuthenticatedDataProvider
    {
        string VersionInfo();
        ICollection<Person> MyPeople();
        int AddSessionLog(int sessionId, int personId, string name, string title, string description);
        int AddSession(string title, string name, string description, DateTime dateTime);
        int AddPerson(string shortName, string longName, string description);
        void ChangePicture(int id, byte[] data, string mimeType);
        Page UpdatePageDetails(int id, string shortName, string fullName, string description);    
        Page GetPage(int id);
        ICollection<Page> RecentPages();
        ICollection<Page> MyStuff();
        ICollection<Session> Sessions();
        ICollection<Person> People();
        ICollection<SessionLog> Logs();
        void RemoveProfileImage(int id);
        int AddPage(string shortName, string fullName, string description);
        ICollection<Place> Places();
        int AddPlace(string fullName, string shortName, string description, int? parentId);
        ICollection<Page> PossibleLinks(int id);
        void AddLink(int id, int addLinkTo);
        void RemoveLink(int id, int linkToDeleteId);
        void DeletePage(int id);
        bool PageExists(string shortName, string fullName);
        bool PageExists(string shortName);
        Page GetPage(string shortName);
        ICollection<Page> PinnedPages();
        ICollection<Page> NewPages();
        ICollection<Page> ModifiedPages();
        void PinPage(int id);
        void MarkAsSeen(int id);
        void ResurrectPerson(int id);
        void KillPerson(int id, string obiturary);
        Trophy GetTrophy(int id);
        int AddTrophy(string name, string description, int pointsValue, byte[] imageData, string mimeType);
        void UpdateTrophy(int id, string name, string description, int pointsValue, byte[] imageData, string mimeType);
        void UpdateTrophy(int id, string name, string description, int pointsValue);
        ICollection<Trophy> Trophies();
        void AwardTrophy(int personId, int trophyId, string reason);
        void RemoveAward(int personId, int awardId);
    }
}
