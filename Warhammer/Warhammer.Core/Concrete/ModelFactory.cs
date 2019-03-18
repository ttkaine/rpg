using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;
using Warhammer.Core.RoleplayViewModels;

namespace Warhammer.Core.Concrete
{
    public class ModelFactory : IModelFactory
    {
        private Player _gm = null;
        private CampaignDetail _campaign = null;

        public CampaignDetail Campaign
        {
            get
            {
                if (_campaign == null)
                {
                    _campaign = _repository.CampaignDetails().Single();
                }
                return _campaign;
            }
        }

        public Player Gm
        {
            get
            {
                if (_gm == null)
                {

                    if (Campaign != null)
                    {
                        _gm = _repository.Players().Single(p => p.Id == Campaign.GmId);
                    }
                }
                return _gm;
            }
        }

        private int GetGmId(int sessionId)
        {
            int? sessionGm = _repository.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId)?.GmId;
            if (sessionGm.HasValue)
            {
                return sessionGm.Value;
            }
            return Gm.Id;
        }

        private readonly IAuthenticatedUserProvider _userProvider;
        private IAuthenticatedUserProvider UserProvider
        {
            get { return _userProvider; }
        }

        private readonly IRepository _repository;
        private IRepository Repo
        {
            get { return _repository; }
        }


        public ModelFactory(IAuthenticatedUserProvider userProvider, IRepository repository)
        {
            _userProvider = userProvider;
            _repository = repository;

        }

        public PlayerViewModel GetPlayerForCurrentUser(int sessionId)
        {
            Player player = Repo.Players().FirstOrDefault(p => p.UserName == UserProvider.UserName);

            if (player != null)
            {
                PlayerViewModel viewModel = new PlayerViewModel();
                viewModel.ID = player.Id;
                viewModel.Name = player.DisplayName;
                viewModel.IsGM = player.Id == GetGmId(sessionId);


                return viewModel;
            }
            else
            {
                return null;
            }
        }

        public PlayerViewModel GetPlayer(int playerId, int sessionId)
        {
            Player player = Repo.Players().FirstOrDefault(p => p.Id == playerId);

            if (player != null)
            {
                PlayerViewModel viewModel = new PlayerViewModel();
                viewModel.ID = player.Id;
                viewModel.Name = player.DisplayName;
                viewModel.IsGM = player.Id == GetGmId(sessionId);

                return viewModel;
            }
            else
            {
                return null;
            }
        }

        public List<PlayerViewModel> GetPlayersForSessionExcludingUser(int sessionId, out int gmId)
        {
            List<PlayerViewModel> viewModels = new List<PlayerViewModel>();
            Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
            PlayerViewModel currentPlayer = GetPlayerForCurrentUser(sessionId);

            if (session != null && currentPlayer != null)
            {
                foreach (Player player in Repo.Players())
                {
                    if (player.Id != currentPlayer.ID)
                    {
                        PlayerViewModel viewModel = new PlayerViewModel();
                        viewModel.ID = player.Id;
                        viewModel.Name = player.DisplayName;
                        viewModel.IsGM = player.Id == GetGmId(sessionId);
                        viewModels.Add(viewModel);
                    }
                }
            }

            gmId = GetGmId(sessionId);

            return viewModels;
        }

        public CharacterViewModel GetCharacterForCurrentUser(int characterId, int sessionId)
        {
            PlayerViewModel currentPlayer = GetPlayerForCurrentUser(sessionId);
            Person character = Repo.People().FirstOrDefault(p => p.Id == characterId && (p.PlayerId == currentPlayer.ID || (p.PlayerId == null && currentPlayer.IsGM)));

            if (character != null)
            {
                CharacterViewModel viewModel = new CharacterViewModel();
                viewModel.ID = character.Id;
                viewModel.Description = character.RawText;
                viewModel.Name = character.ShortName;
                viewModel.Image = character.PrimaryImage;
                viewModel.ImageMimeType = character.ImageMime;
                viewModel.PlayerId = character.PlayerId.GetValueOrDefault();
                viewModel.CharacterSheet = string.Empty;

                return viewModel;
            }

            return null;
        }

        private CharacterViewModel GetCharacterViewModel(Person character)
        {
            CharacterViewModel viewModel = new CharacterViewModel();
            viewModel.ID = character.Id;
            viewModel.Description = character.RawText;
            viewModel.Name = character.ShortName;
            viewModel.Image = character.PrimaryImage;
            viewModel.ImageMimeType = character.ImageMime;
            viewModel.PlayerId = character.PlayerId.GetValueOrDefault();
            viewModel.CharacterSheet = string.Empty;

            return viewModel;
        }

        public CharacterViewModel GetCharacter(int characterId)
        {
            Person character = Repo.People().FirstOrDefault(p => p.Id == characterId);

            if (character != null)
            {
                return GetCharacterViewModel(character);
            }

            return null;
        }

        public List<CharacterViewModel> GetCharactersForCurrentUserInSession(int sessionId)
        {
            Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
            PlayerViewModel player = GetPlayerForCurrentUser(sessionId);

            List<CharacterViewModel> viewModels = new List<CharacterViewModel>();
            if (player != null && session != null)
            {
                if (player.IsGM)
                {
                    viewModels.Add(new CharacterViewModel() { ID = 0, Name = "GM" });
                    viewModels.AddRange(session.Npcs.Select(GetCharacterViewModel));
                }

                viewModels.AddRange(from character in session.PlayerCharacters
                                    where character.PlayerId == player.ID
                                    select GetCharacterViewModel(character));

                if (!player.IsGM)
                {
                    viewModels.Add(new CharacterViewModel() { ID = -1, Name = "Environment" });
                }
            }

            return viewModels;
        }

        public SessionViewModel GetSession(int sessionId)
        {
            Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);

            if (session != null)
            {
                SessionViewModel viewModel = new SessionViewModel();
                viewModel.Description = session.Description;
                viewModel.IsClosed = session.IsClosed;
                viewModel.StartDate = session.DateTime.GetValueOrDefault();
                viewModel.Title = session.ShortName;
                viewModel.GmId = GetGmId(sessionId);
                viewModel.CurrentPlayerId = GetCurrentTurnPlayerId(session);
                return viewModel;
            }

            return null;
        }

        public PostViewModel GetPost(int postId, out int playerId, out bool playerIsGm)
        {
            Post post = Repo.Posts().FirstOrDefault(p => p.Id == postId);
            playerIsGm = false;
            playerId = 0;

            if (post != null)
            {
                PlayerViewModel player = GetPlayerForCurrentUser(post.SessionId);
                int gmId = GetGmId(post.SessionId);
                playerId = player.ID;
                playerIsGm = player.IsGM;
                List<int> playerIds = new List<int>();
                StringBuilder names = new StringBuilder();
                if (post.TargetPlayerIds != null)
                {
                    playerIds.AddRange(GetIntsFromString(post.TargetPlayerIds));
                    foreach (int id in playerIds)
                    {
                        PlayerViewModel targetPlayer = GetPlayer(id, post.SessionId);
                        if (targetPlayer != null)
                        {
                            if (names.Length > 0)
                            {
                                names.Append(", ");
                            }
                            names.Append(targetPlayer.Name);
                            if (targetPlayer.IsGM)
                            {
                                names.Append(" (GM)");
                            }
                        }
                    }
                }
                else
                {
                    playerIds.Add(player.ID);
                }

                if (post.PlayerId == player.ID || playerIds.Contains(player.ID) || (player.ID == gmId && post.PostType == (int)PostType.DiceRoll))
                {
                    PageLinkModel character = GetCharacterForPost(post);
                    PostViewModel viewModel = GetPostViewModelForPost(post, gmId, player, character);
                    if (viewModel != null)
                    {
                        viewModel.TargetPlayerNames = names.ToString();
                    }
                    return viewModel;
                }
            }

            return null;
        }

        private PageLinkModel GetCharacterForPost(Post post)
        {
            PageLinkModel character = null;
            if (post.CharacterId.HasValue)
            {
                character = Repo.Pages().Where(p => p.Id == post.CharacterId.Value).Select(p =>
                    new PageLinkModel { Id = p.Id, FullName = p.FullName, ShortName = p.ShortName }).FirstOrDefault();
            }

            return character;
        }

        private int GetCurrentTurnPlayerId(Session session)
        {
            if (!session.IsGmTurn || (session.IsGmTurn && session.GmIsSuspended))
            {
                PostOrder order = session.PostOrders.Where(p => !p.IsSuspended).OrderBy(p => p.LastTurnEnded).FirstOrDefault();
                if (order != null)
                {
                    return order.PlayerId;
                }
            }
            return GetGmId(session.Id);
        }

        private List<int> GetIntsFromString(string intString)
        {
            List<int> ints = new List<int>();
            string[] ids = intString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string id in ids)
            {
                int value;
                if (int.TryParse(id, out value))
                {
                    ints.Add(value);
                }
            }
            return ints;
        }

        public List<PostViewModel> GetPostsForCurrentUserInSessionSinceLast(int sessionId, int lastPostId, out int playerId, out bool playerIsGm)
        {
            int currentPlayerId = Repo.Players().Where(p => p.UserName == UserProvider.UserName).Select(p => p.Id).FirstOrDefault();
            int gmId = GetGmId(sessionId);
            List<PageLinkModel> characters = Repo.Pages().OfType<Person>().Where(s => s.Pages.Any(p => p.Id == sessionId)).Select(p => new PageLinkModel { FullName = p.FullName, ShortName = p.ShortName, Id = p.Id }).ToList();
            List<PlayerViewModel> players = Repo.Players().Select(p => new PlayerViewModel { Name = p.DisplayName, ID = p.Id, IsGM = p.Id == gmId }).ToList();
            PlayerViewModel player = players.Single(p => p.ID == currentPlayerId);
            bool currentplayerIsGm = player.IsGM;

            List<PostViewModel> viewModels = new List<PostViewModel>();

            playerId = player.ID;
            List<Post> posts = Repo.Posts().Where(p => p.SessionId == sessionId && p.Id > lastPostId && !p.IsDeleted).ToList();

            foreach (Post post in posts)
            {
                List<int> playerIds = new List<int>();
                StringBuilder names = new StringBuilder();
                if (post.TargetPlayerIds != null)
                {
                    playerIds.AddRange(GetIntsFromString(post.TargetPlayerIds));
                    foreach (int id in playerIds)
                    {
                        PlayerViewModel targetPlayer = players.FirstOrDefault(p => p.ID == id);
                        if (targetPlayer != null)
                        {
                            if (names.Length > 0)
                            {
                                names.Append(", ");
                            }
                            names.Append(targetPlayer.Name);
                            if (targetPlayer.IsGM)
                            {
                                names.Append(" (GM)");
                            }
                        }
                    }
                }
                else
                {
                    playerIds.Add(player.ID);
                }

                if (post.PlayerId == player.ID || playerIds.Contains(player.ID) || (player.ID == gmId && post.PostType == (int)PostType.DiceRoll))
                {
                    PostViewModel viewModel = GetPostViewModelForPost(post, gmId, players.FirstOrDefault(p => p.ID == post.PlayerId), characters.FirstOrDefault(c => c.Id == post.CharacterId));
                    if (viewModel != null)
                    {
                        viewModel.TargetPlayerNames = names.ToString();
                        viewModels.Add(viewModel);
                    }
                }

            }
            playerIsGm = player.IsGM;

            playerId = currentPlayerId;
            playerIsGm = currentplayerIsGm;
            return viewModels;
        }

        private PostViewModel GetPostViewModelForPost(Post post, int gmId, PlayerViewModel player, PageLinkModel character)
        {
            PostViewModel viewModel;
            if (post.PostType == (int)PostType.DiceRoll)
            {
                DiceRollPostViewModel diceRollViewModel = new DiceRollPostViewModel();
                diceRollViewModel.DieCount = post.DieCount;
                diceRollViewModel.DieSize = post.DieSize;
                diceRollViewModel.RollTarget = post.RollTarget;
                diceRollViewModel.RollType = post.RollType;
                diceRollViewModel.RollValues.AddRange(GetIntsFromString(post.RollValues));
                diceRollViewModel.ReRollMaximums = post.ReRollMaximums;

                viewModel = diceRollViewModel;
            }
            else if (post.PostType == (int)PostType.Image)
            {
                ImagePostViewModel imagePostViewModel = new ImagePostViewModel();
                imagePostViewModel.ImageId = post.ImageId ?? 0;
                imagePostViewModel.FileName = post.OriginalContent;

                viewModel = imagePostViewModel;
            }
            else
            {
                TextPostViewModel textViewModel = new TextPostViewModel();
                if (!post.IsRevised)
                {
                    textViewModel.Content = post.OriginalContent.Replace("{CR}", "<br />");
                    textViewModel.LastEdited = null;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(post.RevisedContent))
                    {
                        textViewModel.Content = post.RevisedContent.Replace("{CR}", "<br />");
                        textViewModel.CanRevert = true;
                    }
                    else
                    {
                        textViewModel.Content = post.OriginalContent.Replace("{CR}", "<br />");
                    }
                    textViewModel.LastEdited = post.LastEdited.GetValueOrDefault().ToString("dd/MM/yyyy HH:mm:ss");
                    textViewModel.IsRevised = true;
                }

                textViewModel.IsOoc = post.PostType == (int)PostType.OutOfCharacter;

                viewModel = textViewModel;
            }

            viewModel.ID = post.Id;
            viewModel.DatePosted = post.DatePosted.ToString("dd/MM/yyyy HH:mm:ss");
            viewModel.IsPostedByGm = post.PlayerId == gmId;
            viewModel.PlayerId = post.PlayerId;
            viewModel.IsPrivate = post.TargetPlayerIds != null;

            if (post.CharacterId != null)
            {
                if (character != null)
                {
                    viewModel.CharacterId = character.Id;
                    viewModel.CharacterName = character.ShortName;
                }
                else
                {
                    if (viewModel.IsPostedByGm)
                    {
                        viewModel.CharacterName = "GM";
                    }
                    else
                    {
                        viewModel.CharacterName = "Unknown";
                    }

                }
            }
            else
            {
                if (viewModel.IsPostedByGm)
                {
                    viewModel.CharacterName = "GM";
                }
                else
                {
                    viewModel.CharacterName = "Environment";
                }
            }

            if (player != null)
            {
                viewModel.PlayerName = player.Name;
            }

            viewModel.PostType = post.PostType;

            return viewModel;
        }

        public List<PostViewModel> GetEditedPostsForCurrentUserInSessionSinceLast(int sessionId, DateTime lastUpdate)
        {
            Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
            PlayerViewModel player = GetPlayerForCurrentUser(sessionId);
            int gmId = GetGmId(sessionId);

            List<PostViewModel> viewModels = new List<PostViewModel>();
            if (player != null && session != null)
            {
                List<Post> posts = Repo.Posts().Where(p => p.SessionId == session.Id && p.LastEdited >= lastUpdate && !p.IsDeleted).ToList();
                foreach (Post post in posts)
                {
                    List<int> playerIds = new List<int>();
                    StringBuilder names = new StringBuilder();
                    if (post.TargetPlayerIds != null)
                    {
                        playerIds.AddRange(GetIntsFromString(post.TargetPlayerIds));
                        foreach (int id in playerIds)
                        {
                            PlayerViewModel targetPlayer = GetPlayer(id, sessionId);
                            if (targetPlayer != null)
                            {
                                if (names.Length > 0)
                                {
                                    names.Append(", ");
                                }
                                names.Append(targetPlayer.Name);
                                if (targetPlayer.IsGM)
                                {
                                    names.Append(" (GM)");
                                }
                            }
                        }
                    }
                    else
                    {
                        playerIds.Add(player.ID);
                    }

                    if (post.PlayerId == player.ID || playerIds.Contains(player.ID) || (player.IsGM && post.PostType == (int)PostType.DiceRoll))
                    {
                        PageLinkModel character = GetCharacterForPost(post);
                        PostViewModel viewModel = GetPostViewModelForPost(post, gmId, player, character);
                        if (viewModel != null)
                        {
                            viewModel.TargetPlayerNames = names.ToString();
                            viewModels.Add(viewModel);
                        }
                    }
                }
            }
            return viewModels;
        }

        public List<int> GetDeletedPostIdsForCurrentUserInSessionSinceLast(int sessionId, DateTime lastUpdate)
        {
            List<Post> posts = Repo.Posts().Where(p => p.SessionId == sessionId && p.DeletedDate >= lastUpdate && p.IsDeleted).ToList();

            List<int> ids = new List<int>();
            foreach (Post post in posts)
            {
                ids.Add(post.Id);
            }

            return ids;
        }

        public PostedImageViewModel GetPostedImage(int imageId)
        {
            PageImage image = Repo.PageImages().FirstOrDefault(i => i.Id == imageId);
            if (image != null && image.Data.Length > 0)
            {
                Bitmap bmp;
                string mimeType;
                try
                {
                    using (MemoryStream stream = new MemoryStream(image.Data))
                    {
                        bmp = new Bitmap(stream);
                    }
                    mimeType = GetImageMimeType(bmp);
                    bmp.Dispose();
                }
                catch
                {
                    mimeType = "image/unknown";
                }

                PostedImageViewModel viewModel = new PostedImageViewModel()
                {
                    ID = image.Id,
                    Image = image.Data,
                    MimeType = mimeType
                };
                return viewModel;
            }
            return null;
        }

        private static string GetImageMimeType(Image i)
        {
            var imgguid = i.RawFormat.Guid;
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
            {
                if (codec.FormatID == imgguid)
                    return codec.MimeType;
            }
            return "image/unknown";
        }
    }
}
