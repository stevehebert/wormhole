﻿using System;
using System.Collections.Generic;
using Wormhole.Configuration;
using Wormhole.DependencyInjection;
using Wormhole.Exceptions;

namespace Wormhole.PipeAndFilter
{
    public class PipelineCompiler : IPipeCompiler
    {
        private readonly IPipeDefinition _pipeDefinition;
 
        public PipelineCompiler(IPipeDefinition pipeDefinition)
        {
            _pipeDefinition = pipeDefinition;
        }

        public Func<IResolveTypes, object, object> Compile()
        {
            if (!_pipeDefinition.Closed)
                throw new MismatchedClosingTypeDeclarationException();

            return Compile(new Queue<IOperation>(_pipeDefinition.Operations));
        }

        public Func<IResolveTypes, object, object> Compile(Queue<IOperation> operations)
        {
            var functionDefinition = operations.Dequeue().GetClosure();
            while (operations.Count > 0)
            {
                var localFn = functionDefinition;
                var fn = operations.Dequeue().GetClosure();

                functionDefinition = (a, b) => fn(a, localFn(a, b));
            }

            return functionDefinition;
        }
    }
}
