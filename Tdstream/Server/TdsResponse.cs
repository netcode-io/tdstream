using FreeTds;
using System;
using System.Data;

namespace Tdstream.Server
{
    /// <summary>
    /// Class TdsResponse.
    /// </summary>
    public class TdsResponse : IDisposable
    {
        enum TableState
        {
            None,
            Column,
            Row,
            Done,
        }

        readonly TdsSocket _client;
        TableState _state;
        bool _inProc;

        /// <summary>
        /// Initializes a new instance of the <see cref="TdsResponse"/> class.
        /// </summary>
        public TdsResponse(TdsSocket client) => _client = client;

        public void Dispose()
        {
            Info?.Dispose();
            Info = null;
        }

        public MarshaledObjectArrayAccessor<TdsResultInfo, TDSRESULTINFO, TdsColumn, TDSCOLUMN> Columns { get; private set; }

        public MarshaledObjectArrayAccessor<TdsResultInfo, TDSRESULTINFO, TdsColumn, TDSCOLUMN> NewTable(int columns) { NextState(TableState.Column, columns); return Columns; }

        public MarshaledObjectArrayAccessor<TdsResultInfo, TDSRESULTINFO, TdsColumn, TDSCOLUMN> NewRow() { NextState(TableState.Row); return Columns; }

        public void Done() => NextState(TableState.Done);

        public TdsResultInfo Info { get; set; }

        public int InfoRows { get; private set; }

        public tds_end InfoDoneFlag { get; set; }

        void NextState(TableState nextState, params object[] args)
        {
            if (_state == TableState.Done)
                throw new InvalidOperationException($"invalid next state: {_state} to {nextState}");
            _client.OutFlag = TDS_PACKET_TYPE.TDS_REPLY;
            switch (nextState)
            {
                case TableState.None: throw new InvalidOperationException($"invalid next state: {_state} to {nextState}");
                case TableState.Column:
                    Info?.Dispose();
                    if (_state == TableState.None) { }
                    else if (_state == TableState.Column)
                        _client.SendDone(!_inProc ? P.TDS_DONE_TOKEN : P.TDS_DONEPROC_TOKEN, InfoDoneFlag | tds_end.TDS_DONE_MORE_RESULTS, InfoRows);
                    else if (_state == TableState.Row)
                    {
                        _client.SendRow(Info);
                        _client.SendDone(!_inProc ? P.TDS_DONE_TOKEN : P.TDS_DONEPROC_TOKEN, InfoDoneFlag | tds_end.TDS_DONE_MORE_RESULTS, InfoRows);
                    }
                    else throw new InvalidOperationException($"invalid next state: {_state} to {nextState}");
                    Info = new TdsResultInfo((int)args[0]);
                    InfoRows = 0;
                    Columns = Info.Columns;
                    InfoDoneFlag = 0;
                    _state = nextState;
                    break;
                case TableState.Row:
                    if (_state == TableState.Column)
                    {
                        _client.SendResult(Info);
                        _client.SendControlToken(1);
                    }
                    else if (_state == TableState.Row)
                        _client.SendRow(Info);
                    else throw new InvalidOperationException($"invalid next state: {_state} to {nextState}");
                    InfoRows++;
                    _state = nextState;
                    break;
                case TableState.Done:
                    if (_state == TableState.Column)
                        _client.SendDone(!_inProc ? P.TDS_DONE_TOKEN : P.TDS_DONEPROC_TOKEN, InfoDoneFlag, InfoRows);
                    else if (_state == TableState.Row)
                    {
                        _client.SendRow(Info);
                        _client.SendDone(!_inProc ? P.TDS_DONE_TOKEN : P.TDS_DONEPROC_TOKEN, InfoDoneFlag, InfoRows);
                    }
                    else throw new InvalidOperationException($"invalid next state: {_state} to {nextState}");
                    Columns = null;
                    InfoDoneFlag = 0;
                    _state = nextState;
                    break;
            }
        }
    }
}
