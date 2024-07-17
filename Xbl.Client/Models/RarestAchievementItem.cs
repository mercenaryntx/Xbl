using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xbl.Models;

public record RarestAchievementItem(string Title, string Achievement, double Percentage);
public record MinutesPlayed(string Title, int Minutes);