using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Services.Interfaces;


namespace Web.Services.Concrete
{
    public class ActiveCodeHelperService : IActiveCodeHelperService
    {

        private RAQ_DbContext _dbContext;
        private IHttpClient _httpClient;
        private IHostingEnvironment _environment;
        private IRepository<User> _userRepo;
        private IRepository<Organization> _orgRepo;
        private IRepository<ServiceLine> _serviceLineRepo;
        private IRepository<UsersSchedule> _userSchedulesRepo;
        private IRepository<ControlListDetail> _controlListDetailsRepo;
        private IRepository<ActiveCode> _activeCodeRepo;
        private IRepository<CodeStroke> _codeStrokeRepo;
        private IRepository<CodeSepsi> _codeSepsisRepo;
        private IRepository<CodeStemi> _codeSTEMIRepo;
        private IRepository<CodeTrauma> _codeTrumaRepo;
        private IRepository<CodeBlue> _codeBlueRepo;
        private IRepository<CodeStrokeGroupMember> _StrokeCodeGroupMembersRepo;
        private IRepository<CodeStemigroupMember> _STEMICodeGroupMembersRepo;
        private IRepository<CodeSepsisGroupMember> _SepsisCodeGroupMembersRepo;
        private IRepository<CodeTraumaGroupMember> _TraumaCodeGroupMembersRepo;
        private IRepository<CodeBlueGroupMember> _BlueCodeGroupMembersRepo;
        private IRepository<CodesServiceLinesMapping> _codesServiceLinesMappingRepo;
        private IRepository<ConversationChannel> _conversationChannelsRepo;

        IConfiguration _config;
        private string _RootPath;
        private string _GoogleApiKey;
        public ActiveCodeHelperService(RAQ_DbContext dbContext,
            IConfiguration config,
            IHttpClient httpClient,
            IHostingEnvironment environment,
            IRepository<User> userRepo,
            IRepository<Organization> orgRepo,
            IRepository<ServiceLine> serviceLineRepo,
            IRepository<UsersSchedule> userSchedulesRepo,
            IRepository<ControlListDetail> controlListDetailsRepo,
            IRepository<ActiveCode> activeCodeRepo,
            IRepository<CodeStroke> codeStrokeRepo,
            IRepository<CodeSepsi> codeSepsisRepo,
            IRepository<CodeStemi> codeSTEMIRepo,
            IRepository<CodeTrauma> codeTrumaRepo,
            IRepository<CodeBlue> codeBlueRepo,
            IRepository<CodeStrokeGroupMember> StrokeCodeGroupMembersRepo,
            IRepository<CodeStemigroupMember> STEMICodeGroupMembersRepo,
            IRepository<CodeSepsisGroupMember> SepsisCodeGroupMembersRepo,
            IRepository<CodeTraumaGroupMember> TraumaCodeGroupMembersRepo,
            IRepository<CodeBlueGroupMember> BlueCodeGroupMembersRepo,
            IRepository<CodesServiceLinesMapping> codesServiceLinesMappingRepo,
            IRepository<ConversationChannel> conversationChannelsRepo)
        {
            this._config = config;
            this._httpClient = httpClient;
            this._dbContext = dbContext;
            this._environment = environment;
            this._userRepo = userRepo;
            this._orgRepo = orgRepo;
            this._serviceLineRepo = serviceLineRepo;
            this._userSchedulesRepo = userSchedulesRepo;
            this._controlListDetailsRepo = controlListDetailsRepo;
            this._activeCodeRepo = activeCodeRepo;
            this._codeStrokeRepo = codeStrokeRepo;
            this._codeSepsisRepo = codeSepsisRepo;
            this._codeSTEMIRepo = codeSTEMIRepo;
            this._codeTrumaRepo = codeTrumaRepo;
            this._codeBlueRepo = codeBlueRepo;

            this._StrokeCodeGroupMembersRepo = StrokeCodeGroupMembersRepo;
            this._STEMICodeGroupMembersRepo = STEMICodeGroupMembersRepo;
            this._SepsisCodeGroupMembersRepo = SepsisCodeGroupMembersRepo;
            this._TraumaCodeGroupMembersRepo = TraumaCodeGroupMembersRepo;
            this._BlueCodeGroupMembersRepo = BlueCodeGroupMembersRepo;

            this._codesServiceLinesMappingRepo = codesServiceLinesMappingRepo;
            this._conversationChannelsRepo = conversationChannelsRepo;

            this._RootPath = this._config["FilePath:Path"].ToString();
            this._GoogleApiKey = this._config["GoogleApi:Key"].ToString();


        }
        public void MemberAddedToConversationChannel(List<ConversationParticipant> channelParticipantsList, string ChannelSid)
        {
            var channel = this._conversationChannelsRepo.Table.FirstOrDefault(ch => ch.ChannelSid == ChannelSid);
            if (this._codeStrokeRepo.Table.Count(i => i.ChannelSid == ChannelSid && i.IsActive != false) > 0)
            {
                List<CodeStrokeGroupMember> membersList = new();
                CodeStrokeGroupMember GroupMember = new();
                foreach (var p in channelParticipantsList)
                {
                    GroupMember = new CodeStrokeGroupMember()
                    {
                        UserIdFk = p.UserIdFk,
                        StrokeCodeIdFk = this._codeStrokeRepo.Table.FirstOrDefault(i => i.ChannelSid == ChannelSid && i.IsActive != false).CodeStrokeId,
                        //ActiveCodeName = UCLEnums.Stroke.ToString(),
                        IsAcknowledge = false,
                        CreatedBy = ApplicationSettings.UserId,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    membersList.Add(GroupMember);

                }
                this._StrokeCodeGroupMembersRepo.Insert(membersList);
            }
            if (this._codeSepsisRepo.Table.Count(i => i.ChannelSid == ChannelSid && i.IsActive != false) > 0)
            {
                List<CodeSepsisGroupMember> membersList = new();
                CodeSepsisGroupMember GroupMember = new();
                foreach (var p in channelParticipantsList)
                {
                    GroupMember = new CodeSepsisGroupMember()
                    {
                        UserIdFk = p.UserIdFk,
                        SepsisCodeIdFk = this._codeSepsisRepo.Table.FirstOrDefault(i => i.ChannelSid == ChannelSid && i.IsActive != false).CodeSepsisId,
                        //ActiveCodeName = UCLEnums.Stroke.ToString(),
                        IsAcknowledge = false,
                        CreatedBy = ApplicationSettings.UserId,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    membersList.Add(GroupMember);

                }
                this._SepsisCodeGroupMembersRepo.Insert(membersList);
            }
            if (this._codeSTEMIRepo.Table.Count(i => i.ChannelSid == ChannelSid && i.IsActive != false) > 0)
            {
                List<CodeStemigroupMember> membersList = new();
                CodeStemigroupMember GroupMember = new();
                foreach (var p in channelParticipantsList)
                {
                    GroupMember = new CodeStemigroupMember()
                    {
                        UserIdFk = p.UserIdFk,
                        StemicodeIdFk = this._codeSTEMIRepo.Table.FirstOrDefault(i => i.ChannelSid == ChannelSid && i.IsActive != false).CodeStemiid,
                        //ActiveCodeName = UCLEnums.Stroke.ToString(),
                        IsAcknowledge = false,
                        CreatedBy = ApplicationSettings.UserId,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    membersList.Add(GroupMember);

                }
                this._STEMICodeGroupMembersRepo.Insert(membersList);
            }
            if (this._codeBlueRepo.Table.Count(i => i.ChannelSid == ChannelSid && i.IsActive != false) > 0)
            {
                List<CodeBlueGroupMember> membersList = new();
                CodeBlueGroupMember GroupMember = new();
                foreach (var p in channelParticipantsList)
                {
                    GroupMember = new CodeBlueGroupMember()
                    {
                        UserIdFk = p.UserIdFk,
                        BlueCodeIdFk = this._codeBlueRepo.Table.FirstOrDefault(i => i.ChannelSid == ChannelSid && i.IsActive != false).CodeBlueId,
                        //ActiveCodeName = UCLEnums.Stroke.ToString(),
                        IsAcknowledge = false,
                        CreatedBy = ApplicationSettings.UserId,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    membersList.Add(GroupMember);

                }
                this._BlueCodeGroupMembersRepo.Insert(membersList);
            }
            if (this._codeTrumaRepo.Table.Count(i => i.ChannelSid == ChannelSid && i.IsActive != false) > 0)
            {
                List<CodeTraumaGroupMember> membersList = new();
                CodeTraumaGroupMember GroupMember = new();
                foreach (var p in channelParticipantsList)
                {
                    GroupMember = new CodeTraumaGroupMember()
                    {
                        UserIdFk = p.UserIdFk,
                        TraumaCodeIdFk = this._codeTrumaRepo.Table.FirstOrDefault(i => i.ChannelSid == ChannelSid && i.IsActive != false).CodeTraumaId,
                        //ActiveCodeName = UCLEnums.Stroke.ToString(),
                        IsAcknowledge = false,
                        CreatedBy = ApplicationSettings.UserId,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    membersList.Add(GroupMember);
                }
                this._TraumaCodeGroupMembersRepo.Insert(membersList);
            }

        }
    }
}
