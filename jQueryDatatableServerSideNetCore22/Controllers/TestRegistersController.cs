using jQueryDatatableServerSideNetCore22.Data;
using jQueryDatatableServerSideNetCore22.Extensions;
using jQueryDatatableServerSideNetCore22.Models.AuxiliaryModels;
using jQueryDatatableServerSideNetCore22.Models.DatabaseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RandomGen;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace jQueryDatatableServerSideNetCore22.Controllers
{
    public class TestRegistersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestRegistersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TestRegisters
        public async Task<IActionResult> Index()
        {
            await SeedData();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoadTable([FromBody]DTParameters dtParameters)
        {
            var searchBy = dtParameters.Search?.Value;

            var orderCriteria = string.Empty;
            var orderAscendingDirection = true;

            if (dtParameters.Order.Length != 0)
            {
                orderCriteria = dtParameters.Columns[dtParameters.Order[0].Column].Data;
                orderAscendingDirection = dtParameters.Order[0].Dir.ToString().ToLower() == "asc";
            }

            #region Example SQL
            //USE[NAME_DATABASE]

            //SET ANSI_NULLS ON
            //GO
            //SET QUOTED_IDENTIFIER ON
            //GO

            //CREATE PROCEDURE List
            //  @OrderBy AS NVARCHAR(100),
            //	@OrderDirection AS NVARCHAR(4),
            //	@SearchBy AS NVARCHAR(100) = '',
            //	@Start AS INT,
            //	@Length AS INT

            //AS
            //BEGIN
            //  SET NOCOUNT ON;

            //  DECLARE @sql AS NVARCHAR(MAX);

            //  SET @sql = 'SELECT *
            //    FROM[Department]
            //    WHERE Name LIKE '' % '+ @SearchBy +' % '' OR
            //      ALIAS LIKE '' % '+ @SearchBy +' % '' OR
            //      Rubrik LIKE '' % '+ @SearchBy +' % ''
            //    ORDER BY '+ @OrderBy +' '+ @OrderDirection +'
            //    OFFSET('+ CONVERT(varchar(5), @Start) +') ROWS FETCH NEXT(' + CONVERT(varchar(5), @Length) + ') ROWS ONLY'

            //execute(@sql)
            //END
            //GO

            //***EXAMPLE DATA***
            //USE[NAME_DATABASE]
            //GO

            //DECLARE @return_value int

            //EXEC  @return_value = [dbo].[List]
            //      @OrderBy = 'Id',
            //		@OrderDirection = 'desc',
            //		@SearchBy = '',
            //		@Start = 0,
            //		@Length = 10

            //SELECT  'Return Value' = @return_value

            //GO
            #endregion

            var result = await _context.TestRegisters.ToListAsync();

            if (!string.IsNullOrEmpty(searchBy))
            {
                result = result.Where(r => r.Name != null && r.Name.ToUpper().Contains(searchBy.ToUpper()) ||
                                           r.FirstSurname != null && r.FirstSurname.ToUpper().Contains(searchBy.ToUpper()) ||
                                           r.SecondSurname != null && r.SecondSurname.ToUpper().Contains(searchBy.ToUpper()) ||
                                           r.Street != null && r.Street.ToUpper().Contains(searchBy.ToUpper()) ||
                                           r.Phone != null && r.Phone.ToUpper().Contains(searchBy.ToUpper()) ||
                                           r.ZipCode != null && r.ZipCode.ToUpper().Contains(searchBy.ToUpper()) ||
                                           r.Country != null && r.Country.ToUpper().Contains(searchBy.ToUpper()) ||
                                           r.Notes != null && r.Notes.ToUpper().Contains(searchBy.ToUpper()))
                    .ToList();
            }

            if (dtParameters.Order.Length != 0)
                result = orderAscendingDirection ? result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Asc).ToList() : result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Desc).ToList();

            // now just get the count of items (without the skip and take) - eg how many could be returned with filtering
            var filteredResultsCount = result.Count();
            var totalResultsCount = await _context.TestRegisters.CountAsync();

            return Json(new
            {
                draw = dtParameters.Draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
                    .Skip(dtParameters.Start)
                    .Take(dtParameters.Length)
                    .ToList()
            });
        }

        public async Task SeedData()
        {
            if (!_context.TestRegisters.Any())
            {
                for (var i = 0; i < 1000; i++)
                {
                    await _context.TestRegisters.AddAsync(new TestRegister
                    {
                        Name = i % 2 == 0 ? Gen.Random.Names.Male()() : Gen.Random.Names.Female()(),
                        FirstSurname = Gen.Random.Names.Surname()(),
                        SecondSurname = Gen.Random.Names.Surname()(),
                        Street = Gen.Random.Names.Full()(),
                        Phone = Gen.Random.PhoneNumbers.WithRandomFormat()(),
                        ZipCode = Gen.Random.Numbers.Integers(10000, 99999)().ToString(),
                        Country = Gen.Random.Countries()(),
                        Notes = Gen.Random.Text.Short()(),
                        CreationDate = Gen.Random.Time.Dates(DateTime.Now.AddYears(-100), DateTime.Now)()
                    });
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}
