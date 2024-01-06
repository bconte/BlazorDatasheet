﻿using System;
using System.Collections.Generic;
using BlazorDatasheet.Core.Data;
using BlazorDatasheet.DataStructures.Geometry;
using BlazorDatasheet.DataStructures.References;
using BlazorDatasheet.Formula.Core;
using BlazorDatasheet.Formula.Core.Interpreter.Functions;
using BlazorDatasheet.Formula.Core.Interpreter.References;

namespace BlazorDatasheet.Test.Formula;

public class TestEnvironment : IEnvironment
{
    private Dictionary<CellPosition, CellValue> _cellValues = new();
    private Dictionary<string, ISheetFunction> _functions = new();
    private Dictionary<string, CellValue> _variables = new();

    public void SetCellValue(int row, int col, object val)
    {
        SetCellValue(new CellPosition(row, col), val);
    }

    public void SetCellValue(CellPosition position, object val)
    {
        _cellValues.TryAdd(position, new CellValue(val));
        _cellValues[position] = new CellValue(val);
    }

    public void RegisterFunction(string name, ISheetFunction functionDefinition)
    {
        var validator = new FunctionParameterValidator();
        validator.ValidateOrThrow(functionDefinition.GetParameterDefinitions());

        if (!_functions.ContainsKey(name))
            _functions.Add(name, functionDefinition);
        _functions[name] = functionDefinition;
    }

    public void SetVariable(string name, object variable)
    {
        SetVariable(name, new CellValue(variable));
    }

    public void SetVariable(string name, CellValue value)
    {
        if (!_variables.ContainsKey(name))
            _variables.Add(name, value);
        _variables[name] = value;
    }

    public CellValue GetCellValue(int row, int col)
    {
        var hasVal = _cellValues.TryGetValue(new CellPosition(row, col), out var val);
        if (hasVal)
            return val;
        return CellValue.Empty;
    }

    public CellValue[][] GetRangeValues(RangeAddress rangeAddress) => GetValuesInRange(rangeAddress.RowStart,
        rangeAddress.RowEnd, rangeAddress.ColStart, rangeAddress.ColEnd);

    public CellValue[][] GetRangeValues(Reference reference)
    {
        if (reference.Kind == ReferenceKind.Range)
        {
            var rangeRef = (RangeReference)reference;
            var rstart = rangeRef.Start.ToRegion();
            var rEnd = rangeRef.End.ToRegion();
            var r = rstart.GetBoundingRegion(rEnd);
            return GetValuesInRange(r.Top, r.Bottom, r.Left, r.Right);
        }

        if (reference.Kind == ReferenceKind.Cell)
        {
            var cellRef = (CellReference)reference;
            return new[] { new[] { GetCellValue(cellRef.Row.RowNumber, cellRef.Col.ColNumber) } };
        }

        return Array.Empty<CellValue[]>();
    }

    private CellValue[][] GetValuesInRange(int r0, int r1, int c0, int c1)
    {
        var h = (r1 - r0) + 1;
        var w = (c1 - c0) + 1;
        var arr = new CellValue[h][];

        for (int i = 0; i < h; i++)
        {
            arr[i] = new CellValue[w];
            for (int j = 0; j < w; j++)
            {
                arr[i][j] = GetCellValue(r0 + i, c0 + j);
            }
        }

        return arr;
    }

    public bool FunctionExists(string functionIdentifier)
    {
        return _functions.ContainsKey(functionIdentifier);
    }

    public ISheetFunction GetFunctionDefinition(string identifierText)
    {
        return _functions[identifierText];
    }

    public bool VariableExists(string variableIdentifier)
    {
        return _variables.ContainsKey(variableIdentifier);
    }

    public CellValue GetVariable(string variableIdentifier)
    {
        return _variables[variableIdentifier];
    }
}