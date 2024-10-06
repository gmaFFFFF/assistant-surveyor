using Akka.Actor;
using Akka.Event;

namespace gmafffff.AssistantSurveyor.FilePost.Акторы.ПочтаФайловая;

public sealed partial class Почта : ReceiveActor {
    public enum СостояниеКаталога {
        НетПодписки,
        Наблюдается,
        Сбой
    }

    private readonly ILoggingAdapter _log = Context.GetLogger();

    public Почта() {
      
    }

    public static Props Props(int magicNumber) {
        return Akka.Actor.Props.Create<Почта>();
    }

    #region Состояние

    #endregion
}