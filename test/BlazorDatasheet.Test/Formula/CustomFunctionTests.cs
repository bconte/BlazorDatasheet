﻿using System;
using System.Collections.Generic;
using BlazorDatasheet.Formula.Core.Interpreter.Functions;
using NUnit.Framework;

namespace BlazorDatasheet.Test.Formula;

public class CustomFunctionTests
{
    private FunctionParameterValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new();
    }


    [Test]
    public void Required_Args_After_Optional_Throws_Exception()
    {
        var defns = new ParameterDefinition[]
        {
            new ParameterDefinition("number_opt", ParameterType.Number, ParameterDimensionality.Scalar,
                ParameterRequirement.Optional),
            new ParameterDefinition("number_opt", ParameterType.Number, ParameterDimensionality.Scalar,
                ParameterRequirement.Required)
        };

        Assert.Throws<InvalidFunctionDefinitionException>(() => { _validator.ValidateOrThrow(defns); });
    }

    [Test]
    public void Repeat_Args_Defined_Before_End_Throws_Exception()
    {
        var defns = new ParameterDefinition[]
        {
            new ParameterDefinition("number_optional",
                ParameterType.Number,
                ParameterDimensionality.Scalar,
                ParameterRequirement.Required,
                true),
            new ParameterDefinition("number_optiona1",
                ParameterType.Number,
                ParameterDimensionality.Scalar,
                ParameterRequirement.Required,
                false),
        };

        Assert.Throws<InvalidFunctionDefinitionException>(() => { _validator.ValidateOrThrow(defns); });
    }

    [Test]
    public void Valid_Param_Definition_Does_Not_Throw_Exception()
    {
        var defns = new ParameterDefinition[]
        {
            new ParameterDefinition("number_required", ParameterType.Number, ParameterDimensionality.Range,
                ParameterRequirement.Required),
            new ParameterDefinition("number_optional", ParameterType.Number, ParameterDimensionality.Range,
                ParameterRequirement.Optional),
            new ParameterDefinition("number_repeating", ParameterType.Number, ParameterDimensionality.Range,
                ParameterRequirement.Optional)
        };
        Assert.DoesNotThrow(() => { _validator.ValidateOrThrow(defns); });
    }
}

public class CustomFunctionDefinition : ISheetFunction
{
    private readonly ParameterDefinition[] _parameterDefinitions;

    public CustomFunctionDefinition(params ParameterDefinition[] parameterDefinitions)
    {
        _parameterDefinitions = parameterDefinitions;
    }

    public ParameterDefinition[] GetParameterDefinitions()
    {
        return _parameterDefinitions;
    }

    public object Call(FuncArg[] args)
    {
        return null;
    }

    public bool AcceptsErrors => false;
}