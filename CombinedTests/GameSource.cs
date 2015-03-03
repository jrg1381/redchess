using System.Collections.Generic;

namespace CombinedTests
{
    public class GameSource
    {
        public IEnumerable<string> Games
        {
            get { 
                yield return @"1. e4 e5 2. Nf3 Nc6 3. Bb5 a6 4. Bxc6 dxc6 5. d3 Bg4 6. O-O Bxf3 7. Qxf3 f6
8. Nc3 Bb4 9. Bd2 Ne7 10. a3 Bxc3 11. Bxc3 O-O 12. Rad1 b5 13. d4 exd4 14. Bxd4
Qe8 15. Bc5 Rd8 16. Qf4 Rd7 17. Rd3 Ng6 18. Qf5 Rff7 19. Rfd1 Rxd3 20. Rxd3
Qe5 21. Rd8+ Nf8 22. Qc8 Qxc5 23. h3 Qxc2 24. b4 Qxe4 25. f3 Qe3+ 26. Kh2 h6 27. Qxa6
Qf4+ 28. Kg1 Re7 29. Rd1 Ng6 30. Qa8+ Kh7 31. Qxc6 Qe3+ 32. Kh2 Qf4+ 33. Kh1 Re1+ 1-0";

                yield return @"1. e4 e6 2. d4 d5 3. Nc3 Nf6 4. Bg5 Bb4 5. exd5 Qxd5 6. Bxf6 gxf6 7. Qd2 Bxc3
8. Qxc3 Nc6 9. Nf3 Qe4+ 10. Kd2 Bd7 11. Rd1 O-O-O 12. Kc1 e5 13. Bb5 Nxd4 14.
Nxd4 exd4 15. Rxd4 Qxg2 16. Bxd7+ Rxd7 17. Rxd7 Qxh1+ 18. Rd1 Qxh2 19. Qxf6 Rf8
20. a4 a6 21. Kb1 Qh5 22. Rd3 Re8 23. Qd4 Kb8 24. Qd7 Qh1+ 25. Ka2 Qe4 26. Qxf7
Qxa4+ 27. Ra3 Qc6 28. Qxh7 Qd5+ 29. b3 Re2 30. Ra4 Rxf2 31. Qh8+ Ka7 32. Kb2 c5
33. Rc4 Rf1 34. Ka2 Rf7 35. Qc8 b6 36. Rg4 Qd7 37. Qxd7+ Rxd7 38. Rf4 b5 39.
Rf6 Rd5 40. Kb2 Kb7 41. Rh6 b4 42. Rh7+ Kc6 43. Rh6+ Rd6 44. Rh8 Kb5 45. Rb8+
Rb6 46. Rc8 a5 47. c4+ bxc3+ 48. Kxc3 1/2-1/2";
            }
        }
    }
}