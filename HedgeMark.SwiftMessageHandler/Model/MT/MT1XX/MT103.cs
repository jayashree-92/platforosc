namespace HedgeMark.SwiftMessageHandler.Model.MT.MT1XX
{

    /**
     * <strong>MT 103 - Single Customer Credit Transfer</strong>
     *
     * <p>
     * SWIFT MT103 (ISO 15022) message structure:
     * <br>
     *<div class="scheme"><ul>
     *   <li class="field">Field 20  (M)</li>
     *   <li class="field">Field 13 C (O) (repetitive)</li>
     *   <li class="field">Field 23 B (M)</li>
     *   <li class="field">Field 23 E (O) (repetitive)</li>
     *   <li class="field">Field 26 T (O)</li>
     *   <li class="field">Field 32 A (M)</li>
     *   <li class="field">Field 33 B (O)</li>
     *   <li class="field">Field 36  (O)</li>
     *   <li class="field">Field 50 A,F,K (M)</li>
     *   <li class="field">Field 51 A (O)</li>
     *   <li class="field">Field 52 A,D (O)</li>
     *   <li class="field">Field 53 A,B,D (O)</li>
     *   <li class="field">Field 54 A,B,D (O)</li>
     *   <li class="field">Field 55 A,B,D (O)</li>
     *   <li class="field">Field 56 A,C,D (O)</li>
     *   <li class="field">Field 57 A,B,C,D (O)</li>
     *   <li class="field">Field 59 A,F,NONE (M)</li>
     *   <li class="field">Field 70  (O)</li>
     *   <li class="field">Field 71 A (M)</li>
     *   <li class="field">Field 71 F (O) (repetitive)</li>
     *   <li class="field">Field 71 G (O)</li>
     *   <li class="field">Field 72  (O)</li>
     *   <li class="field">Field 77 B (O)</li>
     *   </ul></div>
    **/

    public class MT103 : AbstractMT
    {
        public MT103() : base(MTDirectory.MT_103)
        {
        }


        public MT103(string sender, string receiver) : base(MTDirectory.MT_103)
        {
            setSenderAndReceiver(sender, receiver);
        }

    }
}
