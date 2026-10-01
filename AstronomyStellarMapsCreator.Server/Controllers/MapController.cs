namespace AstronomyStellarMapsCreator.Server.Controllers
{
    [ApiController]
    [Route("api")]
    public class MapController : ControllerBase
    {
        private IRepository _repository;


        public MapController(IRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("getCat/{id?}")]
        public async Task<ActionResult<List<StarDTO>>> GetCatObjects(int? id)
        {
            var stars = await _repository.GetCatAsync(id);
            return Ok(stars);
        }

        [HttpGet("getCelestialObject/{id}")]
        public async Task<ActionResult<StarDTO>> GetCelestialObject(int id)
        {
            var star = await _repository.GetCelestialObjectAsync(id);

            if (star == null)
                return NotFound();

            return Ok(star);
        }
    }
}
