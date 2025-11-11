using Backend.Areas.Identity.Data;
using Backend.Models.Questions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public SeedController(AuthDbContext context)
        {
            _context = context;
        }

        [HttpPost("questions")]
        public async Task<IActionResult> SeedQuestions()
        {
            try
            {
                // Check if questions already exist
                var existingQuestions = await _context.Questions.CountAsync();
                if (existingQuestions > 0)
                {
                    return Ok(new { message = $"Questions already seeded! ({existingQuestions} questions exist)" });
                }

                var questions = new List<Question>
                {
                    new Question
                    {
                        QuestionText = "Care este capitala Franței?",
                        FakeQuestionText = "Care este capitala Germaniei?",
                        Category = "Geografie",
                        Difficulty = 1,
                        IsActive = true
                    },
                    new Question
                    {
                        QuestionText = "Cine a scris 'Romeo și Julieta'?",
                        FakeQuestionText = "Cine a scris 'Hamlet'?",
                        Category = "Literatură",
                        Difficulty = 2,
                        IsActive = true
                    },
                    new Question
                    {
                        QuestionText = "Care este cel mai mare ocean din lume?",
                        FakeQuestionText = "Care este cel mai adânc ocean din lume?",
                        Category = "Geografie",
                        Difficulty = 1,
                        IsActive = true
                    },
                    new Question
                    {
                        QuestionText = "În ce an a avut loc Revoluția Franceză?",
                        FakeQuestionText = "În ce an a avut loc Revoluția Rusă?",
                        Category = "Istorie",
                        Difficulty = 3,
                        IsActive = true
                    },
                    new Question
                    {
                        QuestionText = "Care este formula chimică a apei?",
                        FakeQuestionText = "Care este formula chimică a oxigenului?",
                        Category = "Științe",
                        Difficulty = 1,
                        IsActive = true
                    },
                    new Question
                    {
                        QuestionText = "Cine a pictat 'Mona Lisa'?",
                        FakeQuestionText = "Cine a pictat 'Ultima Cină'?",
                        Category = "Artă",
                        Difficulty = 2,
                        IsActive = true
                    },
                    new Question
                    {
                        QuestionText = "Care este cel mai mare mamifer din lume?",
                        FakeQuestionText = "Care este cel mai mare animal terestru din lume?",
                        Category = "Biologie",
                        Difficulty = 1,
                        IsActive = true
                    },
                    new Question
                    {
                        QuestionText = "În ce țară se află Turnul Eiffel?",
                        FakeQuestionText = "În ce țară se află Turnul din Pisa?",
                        Category = "Geografie",
                        Difficulty = 1,
                        IsActive = true
                    },
                    new Question
                    {
                        QuestionText = "Câte continente există pe Pământ?",
                        FakeQuestionText = "Câte oceane există pe Pământ?",
                        Category = "Geografie",
                        Difficulty = 1,
                        IsActive = true
                    },
                    new Question
                    {
                        QuestionText = "Care este cel mai rapid animal terestru?",
                        FakeQuestionText = "Care este cel mai rapid animal din apă?",
                        Category = "Biologie",
                        Difficulty = 2,
                        IsActive = true
                    },
                    new Question
                    {
                        QuestionText = "Cine a inventat becul electric?",
                        FakeQuestionText = "Cine a inventat telefonul?",
                        Category = "Istorie",
                        Difficulty = 2,
                        IsActive = true
                    },
                    new Question
                    {
                        QuestionText = "Care este cel mai mare deșert din lume?",
                        FakeQuestionText = "Care este cel mai cald deșert din lume?",
                        Category = "Geografie",
                        Difficulty = 2,
                        IsActive = true
                    },
                    new Question
                    {
                        QuestionText = "În ce an a ajuns omul pe Lună?",
                        FakeQuestionText = "În ce an a fost lansat primul satelit?",
                        Category = "Istorie",
                        Difficulty = 3,
                        IsActive = true
                    },
                    new Question
                    {
                        QuestionText = "Care este cel mai lung râu din lume?",
                        FakeQuestionText = "Care este cel mai lat râu din lume?",
                        Category = "Geografie",
                        Difficulty = 2,
                        IsActive = true
                    },
                    new Question
                    {
                        QuestionText = "Cine a compus 'Simfonia a 9-a'?",
                        FakeQuestionText = "Cine a compus 'Simfonia a 5-a'?",
                        Category = "Muzică",
                        Difficulty = 3,
                        IsActive = true
                    }
                };

                await _context.Questions.AddRangeAsync(questions);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Successfully seeded {questions.Count} questions!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error seeding questions: {ex.Message}" });
            }
        }

        [HttpPost("words")]
        public async Task<IActionResult> SeedWords()
        {
            try
            {
                // Check if words already exist
                var existingWords = await _context.WordHiddens.CountAsync();
                if (existingWords > 0)
                {
                    return Ok(new { message = $"Words already seeded! ({existingWords} words exist)" });
                }

                var words = new List<WordHidden>
                {
                    new WordHidden { Word = "Pisică", FakeWord = "Impostor", Category = "Animale", Difficulty = 1, IsActive = true },
                    new WordHidden { Word = "Mașină", FakeWord = "Impostor", Category = "Transport", Difficulty = 1, IsActive = true },
                    new WordHidden { Word = "Pizza", FakeWord = "Impostor", Category = "Mâncare", Difficulty = 1, IsActive = true },
                    new WordHidden { Word = "Fotbal", FakeWord = "Impostor", Category = "Sport", Difficulty = 1, IsActive = true },
                    new WordHidden { Word = "Telefon", FakeWord = "Impostor", Category = "Tehnologie", Difficulty = 1, IsActive = true },
                    new WordHidden { Word = "Carte", FakeWord = "Impostor", Category = "Educație", Difficulty = 1, IsActive = true },
                    new WordHidden { Word = "Munte", FakeWord = "Impostor", Category = "Natură", Difficulty = 1, IsActive = true },
                    new WordHidden { Word = "Ocean", FakeWord = "Impostor", Category = "Natură", Difficulty = 1, IsActive = true },
                    new WordHidden { Word = "Avion", FakeWord = "Impostor", Category = "Transport", Difficulty = 1, IsActive = true },
                    new WordHidden { Word = "Chitară", FakeWord = "Impostor", Category = "Muzică", Difficulty = 2, IsActive = true },
                    new WordHidden { Word = "Elefant", FakeWord = "Impostor", Category = "Animale", Difficulty = 1, IsActive = true },
                    new WordHidden { Word = "Ciocolată", FakeWord = "Impostor", Category = "Mâncare", Difficulty = 1, IsActive = true },
                    new WordHidden { Word = "Calculator", FakeWord = "Impostor", Category = "Tehnologie", Difficulty = 1, IsActive = true },
                    new WordHidden { Word = "Fotografie", FakeWord = "Impostor", Category = "Artă", Difficulty = 2, IsActive = true },
                    new WordHidden { Word = "Bibliotecă", FakeWord = "Impostor", Category = "Educație", Difficulty = 2, IsActive = true }
                };

                await _context.WordHiddens.AddRangeAsync(words);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Successfully seeded {words.Count} words!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error seeding words: {ex.Message}" });
            }
        }

        [HttpPost("all")]
        public async Task<IActionResult> SeedAll()
        {
            try
            {
                await SeedQuestions();
                await SeedWords();
                return Ok(new { message = "Successfully seeded all data!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error seeding data: {ex.Message}" });
            }
        }
    }
}
