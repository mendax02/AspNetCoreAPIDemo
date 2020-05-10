using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    // [Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    [ApiVersion("2.0")]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _campRepository = campRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                var camps = await _campRepository.GetAllCampsAsync(includeTalks);

                //CampModel[] campModels = _mapper.Map<CampModel[]>(camps);

                return _mapper.Map<CampModel[]>(camps);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Cannot fetch Data {ex.Message}");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var camp = await _campRepository.GetCampAsync(moniker);
                if (camp == null) return NotFound();
                //CampModel campModels = _mapper.Map<CampModel>(camp);

                return Ok(_mapper.Map<CampModel>(camp));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Cannot fetch Data");
            }
        }

        [HttpGet("{moniker}")]
        [MapToApiVersion("1.1")]
        public async Task<ActionResult<CampModel>> GetV2(string moniker)
        {
            try
            {
                var camp = await _campRepository.GetCampAsync(moniker, true);
                if (camp == null) return NotFound();
                //CampModel campModels = _mapper.Map<CampModel>(camp);

                return Ok(_mapper.Map<CampModel>(camp));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Cannot fetch Data");
            }
        }

        [HttpGet("search/{theDate}")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await _campRepository.GetAllCampsByEventDate(theDate, includeTalks);
                if (!results.Any()) NotFound();

                return _mapper.Map<CampModel[]>(results);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Cannot fetch Data");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CampModel>> Post(CampModel campModel)
        {
            try
            {
                var oldCamp = await _campRepository.GetCampAsync(campModel.Moniker);
                if (oldCamp != null) return BadRequest("Already Exists");
                var location = _linkGenerator.GetPathByAction("Get", "Camps", new { campModel.Moniker });

                if (string.IsNullOrEmpty(location)) return BadRequest("Could not use current moniker");

                var camp = _mapper.Map<Camp>(campModel);
                _campRepository.Add(camp);
                if (await _campRepository.SaveChangesAsync())
                    return Created($"/api/camps/{campModel.Moniker}", _mapper.Map<CampModel>(camp));
            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Cannot Create");
            }

            return StatusCode(StatusCodes.Status400BadRequest);
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model)
        {
            try
            {
                var oldCamp = await _campRepository.GetCampAsync(moniker);
                if (oldCamp == null)
                {
                    return NotFound("Moniker doesn't exist!");
                }

                model.Moniker = oldCamp.Moniker;
                _mapper.Map(model, oldCamp);
                if (await _campRepository.SaveChangesAsync())
                    return _mapper.Map<CampModel>(oldCamp);
            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Cannot Create");
            }

            return StatusCode(StatusCodes.Status304NotModified);
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = await _campRepository.GetCampAsync(moniker);
                if (oldCamp == null)
                {
                    return NotFound("Moniker doesn't exist!");
                }
                _campRepository.Delete(oldCamp);
                if (await _campRepository.SaveChangesAsync())
                    return Ok();
            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Cannot Delete");
            }

            return StatusCode(StatusCodes.Status304NotModified);
        }
    }
}