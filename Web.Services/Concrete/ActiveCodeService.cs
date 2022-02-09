using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Extensions;
using Web.Services.Helper;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class ActiveCodeService : IActiveCodeService
    {
        private RAQ_DbContext _dbContext;
        private IRepository<Organization> _orgRepo;
        private IRepository<ServiceLine> _serviceLineRepo;
        private IRepository<ControlListDetail> _controlListDetailsRepo;
        private IRepository<ActiveCode> _activeCodeRepo;
        private IRepository<CodeStroke> _codeStrokeRepo;
        private IRepository<CodeSepsi> _codeSepsisRepo;
        private IRepository<CodeStemi> _codeSTEMIRepo;
        private IRepository<CodeTrauma> _codeTrumaRepo;
        IConfiguration _config;
        public ActiveCodeService(RAQ_DbContext dbContext,
            IConfiguration config,
            IRepository<ActiveCode> activeCodeRepo,
            IRepository<CodeStroke> codeStrokeRepo,
            IRepository<CodeSepsi> codeSepsisRepo,
            IRepository<CodeStemi> codeSTEMIRepo,
            IRepository<CodeTrauma> codeTrumaRepo)
        {
            this._config = config;
            this._activeCodeRepo = activeCodeRepo;
            this._codeStrokeRepo = codeStrokeRepo;
            this._codeSepsisRepo = codeSepsisRepo;
            this._codeSTEMIRepo = codeSTEMIRepo;
            this._codeTrumaRepo = codeTrumaRepo;
        }

        #region Active Code

        public BaseResponse GetActivatedCodesByOrgId(int orgId) 
        {
            var codes = (from c in this._activeCodeRepo.Table
                         join ucl in this._controlListDetailsRepo.Table on c.CodeIdFk equals ucl.ControlListDetailId
                         where c.OrganizationIdFk == orgId && !c.IsDeleted
                         select new ActiveCodeVM()
                         {

                             ActiveCodeId = c.ActiveCodeId,
                             OrganizationIdFk = c.OrganizationIdFk,
                             ActiveCodeName = ucl.Title,
                             CodeIdFk = c.CodeIdFk,
                             ServiceLineIds = c.ServiceLineIds,
                             serviceLines = this._serviceLineRepo.Table.Where(x => !x.IsDeleted && c.ServiceLineIds.ToIntList().Contains(x.ServiceLineId)).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName }).ToList()

                         }).Distinct().ToList();
            if (codes.Count > 0)
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = codes };
            }
            else 
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "No Active Code Found", Body = codes };
            }
        }

        public BaseResponse MapActiveCodes(List<ActiveCodeVM> activeCodes)
        {
            List<ActiveCode> update = new();
            List<ActiveCode> insert = new();
            foreach (var item in activeCodes)
            {
                if (item.ActiveCodeId > 0)
                {
                    var row = this._activeCodeRepo.Table.Where(x => x.ActiveCodeId == item.ActiveCodeId && !x.IsDeleted).FirstOrDefault();
                    row.ServiceLineIds = item.ServiceLineIds;
                    row.ModifiedBy = item.ModifiedBy;
                    row.ModifiedDate = DateTime.Now;
                    row.IsDeleted = false;
                    update.Add(row);
                }
                else
                {
                    item.CreatedDate = DateTime.UtcNow;
                    var row = AutoMapperHelper.MapSingleRow<ActiveCodeVM, ActiveCode>(item);
                    insert.Add(row);
                }

            }
            if (update.Count > 0)
            {
                this._activeCodeRepo.Update(update);
            }
            if (insert.Count > 0)
            {
                this._activeCodeRepo.Insert(insert);
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Saved" };
        }

        public BaseResponse DetachActiveCodes(int activeCodeId)
        {
            var row = this._activeCodeRepo.Table.Where(x => x.ActiveCodeId == activeCodeId && !x.IsDeleted).FirstOrDefault();
            row.ModifiedBy = ApplicationSettings.UserId;
            row.ModifiedDate = DateTime.UtcNow;
            row.IsDeleted = true;
            this._activeCodeRepo.Update(row);
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }


        #endregion

        #region Code Stroke

        public BaseResponse GetAllStrokeCode()
        {
            var strokeData = this._codeStrokeRepo.Table.Where(x => !x.IsDeleted).ToList();
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = strokeData };
        }

        public BaseResponse GetStrokeDataById(int strokeId)
        {
            var strokeData = this._codeStrokeRepo.Table.Where(x => x.CodeStrokeId == strokeId && !x.IsDeleted).FirstOrDefault();
            if (strokeData != null)
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Found", Body = strokeData };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
        }

        public BaseResponse AddOrUpdateStrokeData(CodeStrokeVM codeStroke)
        {
            if (codeStroke.CodeStrokeId > 0)
            {
                var row = this._codeStrokeRepo.Table.Where(x => x.CodeStrokeId == codeStroke.CodeStrokeId && !x.IsDeleted).FirstOrDefault();

                row.PatientName = codeStroke.PatientName;
                row.Dob = codeStroke.Dob;
                row.Gender = codeStroke.Gender;
                row.ChiefComplant = codeStroke.ChiefComplant;
                row.LastKnownWell = codeStroke.LastKnownWell;
                row.Hpi = codeStroke.Hpi;
                row.BloodThinners = codeStroke.BloodThinners;
                row.FamilyContactName = codeStroke.FamilyContactName;
                row.FamilyContactNumber = codeStroke.FamilyContactNumber;
                row.IsEms = codeStroke.IsEms;
                row.IsCompleted = codeStroke.IsCompleted;
                row.ModifiedBy = codeStroke.ModifiedBy;
                row.ModifiedDate = DateTime.UtcNow;
                row.IsDeleted = false;
                this._codeStrokeRepo.Update(row);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Modified", Body = row };
            }
            else
            {

                codeStroke.CreatedDate = DateTime.UtcNow;
                var stroke = AutoMapperHelper.MapSingleRow<CodeStrokeVM, CodeStroke>(codeStroke);
                this._codeStrokeRepo.Insert(stroke);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Added", Body = stroke };
            }
        }

        public BaseResponse DeleteStroke(int strokeId)
        {
            var row = this._codeStrokeRepo.Table.Where(x => x.CodeStrokeId == strokeId && !x.IsDeleted).FirstOrDefault();
            row.IsDeleted = true;
            row.ModifiedBy = ApplicationSettings.UserId;
            row.ModifiedDate = DateTime.UtcNow;
            this._codeStrokeRepo.Update(row);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }

        #endregion

        #region Code Sepsis


        public BaseResponse GetAllSepsisCode()
        {
            var SepsisData = this._codeSepsisRepo.Table.Where(x => !x.IsDeleted).ToList();
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = SepsisData };
        }

        public BaseResponse GetSepsisDataById(int SepsisId)
        {
            var SepsisData = this._codeSepsisRepo.Table.Where(x => x.CodeSepsisId == SepsisId && !x.IsDeleted).FirstOrDefault();
            if (SepsisData != null)
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Found", Body = SepsisData };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
        }

        public BaseResponse AddOrUpdateSepsisData(CodeSepsisVM codeSepsis)
        {
            if (codeSepsis.CodeSepsisId > 0)
            {
                var row = this._codeSepsisRepo.Table.Where(x => x.CodeSepsisId == codeSepsis.CodeSepsisId && !x.IsDeleted).FirstOrDefault();

                row.PatientName = codeSepsis.PatientName;
                row.Dob = codeSepsis.Dob;
                row.Gender = codeSepsis.Gender;
                row.ChiefComplant = codeSepsis.ChiefComplant;
                row.LastKnownWell = codeSepsis.LastKnownWell;
                row.Hpi = codeSepsis.Hpi;
                row.BloodThinners = codeSepsis.BloodThinners;
                row.FamilyContactName = codeSepsis.FamilyContactName;
                row.FamilyContactNumber = codeSepsis.FamilyContactNumber;
                row.IsEms = codeSepsis.IsEms;
                row.IsCompleted = codeSepsis.IsCompleted;
                row.ModifiedBy = codeSepsis.ModifiedBy;
                row.ModifiedDate = DateTime.UtcNow;
                row.IsDeleted = false;
                this._codeSepsisRepo.Update(row);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Modified", Body = row };
            }
            else
            {

                codeSepsis.CreatedDate = DateTime.UtcNow;
                var Sepsis = AutoMapperHelper.MapSingleRow<CodeSepsisVM, CodeSepsi>(codeSepsis);
                this._codeSepsisRepo.Insert(Sepsis);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Added", Body = Sepsis };
            }
        }

        public BaseResponse DeleteSepsis(int SepsisId)
        {
            var row = this._codeSepsisRepo.Table.Where(x => x.CodeSepsisId == SepsisId && !x.IsDeleted).FirstOrDefault();
            row.IsDeleted = true;
            row.ModifiedBy = ApplicationSettings.UserId;
            row.ModifiedDate = DateTime.UtcNow;
            this._codeSepsisRepo.Update(row);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }


        #endregion


        #region Code STEMI

        public BaseResponse GetAllSTEMICode()
        {
            var STEMIData = this._codeSTEMIRepo.Table.Where(x => !x.IsDeleted).ToList();
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = STEMIData };
        }

        public BaseResponse GetSTEMIDataById(int STEMIId)
        {
            var STEMIData = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == STEMIId && !x.IsDeleted).FirstOrDefault();
            if (STEMIData != null)
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Found", Body = STEMIData };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
        }

        public BaseResponse AddOrUpdateSTEMIData(CodeSTEMIVM codeSTEMI)
        {
            if (codeSTEMI.CodeStemiid > 0)
            {
                var row = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == codeSTEMI.CodeStemiid && !x.IsDeleted).FirstOrDefault();

                row.PatientName = codeSTEMI.PatientName;
                row.Dob = codeSTEMI.Dob;
                row.Gender = codeSTEMI.Gender;
                row.ChiefComplant = codeSTEMI.ChiefComplant;
                row.LastKnownWell = codeSTEMI.LastKnownWell;
                row.Hpi = codeSTEMI.Hpi;
                row.BloodThinners = codeSTEMI.BloodThinners;
                row.FamilyContactName = codeSTEMI.FamilyContactName;
                row.FamilyContactNumber = codeSTEMI.FamilyContactNumber;
                row.IsEms = codeSTEMI.IsEms;
                row.IsCompleted = codeSTEMI.IsCompleted;
                row.ModifiedBy = codeSTEMI.ModifiedBy;
                row.ModifiedDate = DateTime.UtcNow;
                row.IsDeleted = false;
                this._codeSTEMIRepo.Update(row);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Modified", Body = row };
            }
            else
            {

                codeSTEMI.CreatedDate = DateTime.UtcNow;
                var STEMI = AutoMapperHelper.MapSingleRow<CodeSTEMIVM, CodeStemi>(codeSTEMI);
                this._codeSTEMIRepo.Insert(STEMI);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Added", Body = STEMI };
            }
        }

        public BaseResponse DeleteSTEMI(int STEMIId)
        {
            var row = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == STEMIId && !x.IsDeleted).FirstOrDefault();
            row.IsDeleted = true;
            row.ModifiedBy = ApplicationSettings.UserId;
            row.ModifiedDate = DateTime.UtcNow;
            this._codeSTEMIRepo.Update(row);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }


        #endregion


        #region Code Truma

        public BaseResponse GetAllTrumaCode()
        {
            var TrumaData = this._codeTrumaRepo.Table.Where(x => !x.IsDeleted).ToList();
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = TrumaData };
        }

        public BaseResponse GetTrumaDataById(int TrumaId)
        {
            var TrumaData = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == TrumaId && !x.IsDeleted).FirstOrDefault();
            if (TrumaData != null)
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Found", Body = TrumaData };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
        }

        public BaseResponse AddOrUpdateTrumaData(CodeTrumaVM codeTruma)
        {
            if (codeTruma.CodeTraumaId > 0)
            {
                var row = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == codeTruma.CodeTraumaId && !x.IsDeleted).FirstOrDefault();

                row.PatientName = codeTruma.PatientName;
                row.Dob = codeTruma.Dob;
                row.Gender = codeTruma.Gender;
                row.ChiefComplant = codeTruma.ChiefComplant;
                row.LastKnownWell = codeTruma.LastKnownWell;
                row.Hpi = codeTruma.Hpi;
                row.BloodThinners = codeTruma.BloodThinners;
                row.FamilyContactName = codeTruma.FamilyContactName;
                row.FamilyContactNumber = codeTruma.FamilyContactNumber;
                row.IsEms = codeTruma.IsEms;
                row.IsCompleted = codeTruma.IsCompleted;
                row.ModifiedBy = codeTruma.ModifiedBy;
                row.ModifiedDate = DateTime.UtcNow;
                row.IsDeleted = false;
                this._codeTrumaRepo.Update(row);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Modified", Body = row };
            }
            else
            {

                codeTruma.CreatedDate = DateTime.UtcNow;
                var Truma = AutoMapperHelper.MapSingleRow<CodeTrumaVM, CodeTrauma>(codeTruma);
                this._codeTrumaRepo.Insert(Truma);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Added", Body = Truma };
            }
        }

        public BaseResponse DeleteTruma(int TrumaId)
        {
            var row = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == TrumaId && !x.IsDeleted).FirstOrDefault();
            row.IsDeleted = true;
            row.ModifiedBy = ApplicationSettings.UserId;
            row.ModifiedDate = DateTime.UtcNow;
            this._codeTrumaRepo.Update(row);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }


        #endregion

    }
}
