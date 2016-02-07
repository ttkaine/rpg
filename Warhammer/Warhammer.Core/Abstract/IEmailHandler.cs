﻿using System.Collections.Generic;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Abstract
{
    public interface IEmailHandler
    {
        void NotifyPlayerTurn(Session session, Player player);
        void NotifyNewPage(Page page, List<Player> players);
        void NotifyEditPage(Page page, List<Player> players);
        void NotifyNewComment(string senderName, Page page, List<Player> players);
    }
}
