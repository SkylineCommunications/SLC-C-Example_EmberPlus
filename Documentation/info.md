# EmberPlus SDF

> Version info
>
> - Version: 1.0.0.x
> - low level
> - Polling via SLProtocol/SLPort
> - XML and smart-serial
> - Hard-coded string paths (identifiers) in Parametermapping.cs
>

## One root node

- Should work with one root node:
  path 1 is the root

        <Root Type="ElementCollection">
            <Node Path="1">
                <Contents>
                    <Identifier>R3LAYVirtualPatchBay</Identifier>
                    <Description />
                </Contents>
                <Children>
                    <Node Path="1.0">
                        <Contents>
                            <Identifier>identity</Identifier>
                            <Description />
                        </Contents>
                        <Children>
                            <Parameter Path="1.0.1">
                                <Contents>
                                    <Identifier>product</Identifier>
                                    <Value Type="UTF8">String(R3LAY Virtual PatchBay)</Value>
                                    <Access>1</Access>
                                    <Type>3</Type>
                                </Contents>
                            </Parameter>
                            <Parameter Path="1.0.2">
                                <Contents>
                                    <Identifier>company</Identifier>
                                    <Value Type="UTF8">String(Lawo AG)</Value>
                                    <Access>1</Access>
                                    <Type>3</Type>
                                </Contents>
                            </Parameter>
                            <Parameter Path="1.0.3">
                                <Contents>
                                    <Identifier>version</Identifier>
                                    <Value Type="UTF8">String(4.1.0.65, 23.06.2020)</Value>
                                    <Access>1</Access>
                                    <Type>3</Type>
                                </Contents>
                            </Parameter>
                            <Parameter Path="1.0.4">
                                <Contents>
                                    <Identifier>role</Identifier>
                                    <Value Type="UTF8">String(SRFC006788)</Value>
                                    <Access>1</Access>
                                    <Type>3</Type>
                                </Contents>
                            </Parameter>
                            <Parameter Path="1.0.5">
                                <Contents>
                                    <Identifier>serial</Identifier>
                                    <Value Type="UTF8">String(SRFC006788 (10.120.98.98))</Value>
                                    <Access>1</Access>
                                    <Type>3</Type>
                                </Contents>
                            </Parameter>
                        </Children>
                    </Node>
                </Children>
            </Node>
        </Root>

## Multiple root nodes

- TODO: Let it work with multiple root nodes:
  path 0 and 1 are root nodes

        <Root Type="ElementCollection">
            <Node Path="0">
                <Contents>
                    <Identifier>log</Identifier>
                </Contents>
                <Children>
                    <Parameter Path="0.12">
                        <Contents>
                            <Identifier>severityFilter</Identifier>
                            <Value Type="INTEGER">Integer(2)</Value>
                            <Access>3</Access>
                            <Type>1</Type>
                            <Enumeration>
                    info 2
                    warning 3
                    trace 0
                    error 4
                    debug 1
                    fatal 5
                            </Enumeration>
                        </Contents>
                    </Parameter>
                </Children>
            </Node>
            <Node Path="1">
                <Contents>
                    <Identifier>identity</Identifier>
                </Contents>
                <Children>
                    <Parameter Path="1.13">
                        <Contents>
                            <Identifier>product</Identifier>
                            <Value Type="UTF8">String(A__STAGE)</Value>
                            <Access>1</Access>
                            <Type>3</Type>
                        </Contents>
                    </Parameter>
                    <Parameter Path="1.14">
                        <Contents>
                            <Identifier>version</Identifier>
                            <Value Type="UTF8">String(1.8.0.79)</Value>
                            <Access>1</Access>
                            <Type>3</Type>
                        </Contents>
                    </Parameter>
                    <Parameter Path="1.15">
                        <Contents>
                            <Identifier>role</Identifier>
                            <Value Type="UTF8">String()</Value>
                            <Access>3</Access>
                            <Type>3</Type>
                        </Contents>
                    </Parameter>
                    <Parameter Path="1.16">
                        <Contents>
                            <Identifier>company</Identifier>
                            <Value Type="UTF8">String(Lawo AG)</Value>
                            <Access>1</Access>
                            <Type>3</Type>
                        </Contents>
                    </Parameter>
                    <Parameter Path="1.17">
                        <Contents>
                            <Identifier>serial</Identifier>
                            <Value Type="UTF8">String()</Value>
                            <Access>1</Access>
                            <Type>3</Type>
                        </Contents>
                    </Parameter>
                </Children>
            </Node>
        </Root>
