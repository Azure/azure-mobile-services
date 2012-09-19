using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Foundation;
using Windows.Storage.Streams;
using System.Threading;
using Microsoft.Live;
using System.Diagnostics;

namespace Doto
{
    /// <summary>
    /// This example implementation of IServiceFilter is used to create the
    /// busy/progress indicator feature in Doto by counting outgoing calls from 
    /// the mobile service client and responses from the Mobile Service. If a call
    /// is in progress, Doto shows the progress indicator
    /// </summary>
    public class DotoServiceFilter : IServiceFilter
    {
        private int _callCount = 0;
        private readonly object _countLock = new object();
        private Action<bool> _busyIndicator;

        public DotoServiceFilter(Action<bool> busyIndicator)
        {
            _busyIndicator = busyIndicator;
        }

        public Windows.Foundation.IAsyncOperation<IServiceFilterResponse> Handle(IServiceFilterRequest request,
            IServiceFilterContinuation continuation)
        {
            return HandleAsync(request, continuation).AsAsyncOperation();
        }

        private async Task<IServiceFilterResponse> HandleAsync(IServiceFilterRequest request,
            IServiceFilterContinuation continuation)
        {
            int ic = _callCount;

            bool invokeBusy = false;

            lock (_countLock)
            {
                if (_callCount == 0)
                {
                    invokeBusy = true;
                }
                _callCount++;
            }
         
            if (invokeBusy)
            {
                _busyIndicator.Invoke(true);
            }

            IServiceFilterResponse response = await continuation.Handle(request).AsTask();

            bool invokeIdle = false;

            lock (_countLock)
            {
                if (_callCount == 1)
                {
                    invokeIdle = true;
                }
                _callCount--;
            }

            if (invokeIdle) {
                _busyIndicator.Invoke(false);
            }

            return response;
        }
    }
}