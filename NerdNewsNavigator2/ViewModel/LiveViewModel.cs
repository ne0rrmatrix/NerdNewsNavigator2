using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdNewsNavigator2.ViewModel;

public partial class LiveViewModel : ObservableObject
{
    [ObservableProperty]
    public string _url = "https://www.youtube.com/embed/yQPlcthGEe4?autoplay=1";
}
