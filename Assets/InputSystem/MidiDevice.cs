using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;

// MIDI device class driven by MidiJack plugin

namespace MidiJack2
{
    [InputControlLayout(
        stateType = typeof(MidiDeviceState),
        displayName = "MIDI Device"
    )]
    public sealed class MidiDevice : InputDevice, IInputUpdateCallbackReceiver
    {
        #region Public accessors

        public ButtonControl GetNote(int index)
        {
            return _notes[index];
        }

        public AxisControl GetControl(int index)
        {
            return _controls[index];
        }

        #endregion

        #region Internal objects

        MidiDeviceState _state;
        ButtonControl [] _notes;
        AxisControl [] _controls;
        bool _controlModified;

        #endregion

        #region MIDI event receiver

        public void OnNoteOn(byte note, byte velocity)
        {
            unsafe { _state.notes[note] = velocity; }
            InputSystem.QueueDeltaStateEvent(_notes[note], velocity);
        }

        public void OnNoteOff(byte note)
        {
            unsafe { _state.notes[note] = 0; }
            InputSystem.QueueDeltaStateEvent(_notes[note], (byte)0);
        }

        public void OnControlChange(byte number, byte value)
        {
            unsafe { _state.controls[number] = value; }
            _controlModified = true;
        }

        #endregion

        #region InputDevice implementation

        protected override void FinishSetup()
        {
            base.FinishSetup();

            _notes = new ButtonControl[128];
            _controls = new AxisControl[128];

            for (var i = 0; i < 128; i++)
            {
                _notes[i] = GetChildControl<ButtonControl>("note" + i.ToString("D3"));
                _controls[i] = GetChildControl<AxisControl>("control" + i.ToString("D3"));
            }
        }

        public static MidiDevice current { get; private set; }

        public override void MakeCurrent()
        {
            base.MakeCurrent();
            current = this;
        }

        protected override void OnRemoved()
        {
            base.OnRemoved();
            if (current == this) current = null;
        }

        #endregion

        #region IInputUpdateCallbackReceiver implementation

        public void OnUpdate()
        {
            if (_controlModified)
            {
                InputSystem.QueueStateEvent(this, _state);
                _controlModified = false;
            }
        }

        #endregion
    }
}