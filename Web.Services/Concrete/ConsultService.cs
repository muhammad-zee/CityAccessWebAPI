using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Enums;
using Web.Services.Extensions;
using Web.Services.Helper;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class ConsultService : IConsultService
    {
        private RAQ_DbContext _dbContext;
        private IRepository<ConsultField> _consultFieldRepo;
        private IRepository<OrganizationConsultField> _orgConsultRepo;
        private IRepository<ConsultAcknowledgment> _consultAcknowledgmentRepo;
        IConfiguration _config;
        public ConsultService(RAQ_DbContext dbContext,
            IConfiguration config,
            IRepository<ConsultField> consultFieldRepo,
            IRepository<OrganizationConsultField> orgConsultRepo,
            IRepository<ConsultAcknowledgment> consultAcknowledgmentRepo)
        {
            this._config = config;
            this._consultFieldRepo = consultFieldRepo;
            this._orgConsultRepo = orgConsultRepo;
            this._consultAcknowledgmentRepo = consultAcknowledgmentRepo;
        }

        #region Consult Fields

        public BaseResponse GetAllConsultFields()
        {
            var consultFields = this._consultFieldRepo.Table.Where(x => !x.IsDeleted).ToList();
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = consultFields };
        }

        public BaseResponse GetConsultFeildsForOrg(int OrgId)
        {
            var consultFields = this._consultFieldRepo.Table.Where(x => !x.IsDeleted).ToList();
            var selectedConsultFields = this._orgConsultRepo.Table.Where(x => x.OrganizationIdFk == OrgId && !x.IsDeleted).Select(x => x.ConsultFieldIdFk).ToList();

            var consultFieldVM = AutoMapperHelper.MapList<ConsultField, ConsultFieldsVM>(consultFields);

            foreach (var item in consultFieldVM)
            {
                if (selectedConsultFields.Contains(item.ConsultFieldId))
                {
                    item.IsSelected = true;
                }
                else
                {
                    item.IsSelected = false;
                }
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = consultFieldVM };
        }

        public BaseResponse AddOrUpdateConsultFeilds(ConsultFieldsVM consultField)
        {
            if (consultField.ConsultFieldId > 0)
            {
                var consultFeilds = this._consultFieldRepo.Table.Where(x => x.ConsultFieldId == consultField.ConsultFieldId && !x.IsDeleted).FirstOrDefault();
                if (consultFeilds != null)
                {
                    consultFeilds.FieldName = consultField.FieldName;
                    consultFeilds.FieldType = consultField.FieldType;
                    consultFeilds.FieldDataType = consultField.FieldDataType;
                    consultFeilds.FieldDataLength = consultField.FieldDataLength;
                    consultFeilds.ModifiedBy = consultField.ModifiedBy;
                    consultFeilds.ModifiedDate = DateTime.UtcNow;
                    consultFeilds.IsDeleted = false;

                    this._consultFieldRepo.Update(consultFeilds);
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Updated Successfully" };
                }
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
            }
            else
            {
                var consultFields = new ConsultField()
                {
                    FieldName = consultField.FieldName,
                    FieldType = consultField.FieldType,
                    FieldDataType = consultField.FieldDataType,
                    FieldDataLength = consultField.FieldDataLength,
                    CreatedBy = consultField.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };

                this._consultFieldRepo.Insert(consultFields);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Added Successfully" };
            }
        }

        #endregion


        #region Organization Consult Fields

        public BaseResponse AddOrUpdateOrgConsultFeilds(List<OrgConsultFieldsVM> orgConsultFields)
        {

            var duplicateObj = orgConsultFields;

            var alreadyExistFields = this._orgConsultRepo.Table.Where(x => duplicateObj.Select(y => y.ConsultFieldIdFk).Contains(x.ConsultFieldIdFk) && duplicateObj.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).Select(x => x.OrgConsultFieldId).ToList();
            duplicateObj.RemoveAll(r => alreadyExistFields.Contains(r.OrgConsultFieldId));

            var orgConsults = AutoMapperHelper.MapList<OrgConsultFieldsVM, OrganizationConsultField>(duplicateObj);

            this._orgConsultRepo.Insert(orgConsults);

            alreadyExistFields = this._orgConsultRepo.Table.Where(x => duplicateObj.Select(y => y.ConsultFieldIdFk).Contains(x.ConsultFieldIdFk) && duplicateObj.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).Select(x => x.OrgConsultFieldId).ToList();
            duplicateObj.RemoveAll(r => alreadyExistFields.Contains(r.OrgConsultFieldId));

            var deletedOnes = this._orgConsultRepo.Table.Where(x => !(orgConsultFields.Select(y => y.ConsultFieldIdFk).Contains(x.ConsultFieldIdFk) && orgConsultFields.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted)).ToList();

            int? ModifiedBy = orgConsultFields.Select(x => x.ModifiedBy).FirstOrDefault();

            deletedOnes.ForEach(x => { x.ModifiedBy = ModifiedBy; x.ModifiedDate = DateTime.UtcNow; x.IsDeleted = true; });
            this._orgConsultRepo.Update(deletedOnes);

            return new BaseResponse();
        }

        #endregion
    }
}
