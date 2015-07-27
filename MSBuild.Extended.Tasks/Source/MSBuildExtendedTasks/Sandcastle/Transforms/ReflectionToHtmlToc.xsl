<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1">
	<xsl:param name="html" select="string('Output/html')" />
	<xsl:output method="xml"  omit-xml-declaration="yes" encoding="iso-8859-1" />
  <xsl:preserve-space elements="html body head div span"/>
  <xsl:key name="index" match="/reflection/apis/api" use="@id" />
  
	<xsl:template match="/">
    <html>
      <head>
        <link rel="Stylesheet" href="../styles/toc.css" media="screen" type="text/css" />
        <script type="text/javascript" language="javascript">
          <![CDATA[
          function ToggleSubItems ( sender, nodeId ) {
            var subNodeId = nodeId + "SubItems";
            var subNode = document.getElementById(subNodeId);
            var node = document.getElementById(nodeId);
            if ( node != null ) {
              if ( subNode != null ) {
                if ( subNode.style.display != "none" ) {
                  subNode.style.display = "none";
                  sender.className = "label";
                } else {
                  subNode.style.display = "inline";
                  sender.className = "labelOpen";
                }
              }
            }
          }
        ]]>
        </script>
      </head>
      <body>
        <div class="toc">
          <xsl:choose>
            <xsl:when test="/reflection/apis/api[apidata/@group='root']">
              <xsl:apply-templates select="/reflection/apis/api[apidata/@group='root']"/>
            </xsl:when>
            <xsl:when test="/reflection/apis/api[apidata/@group='namespace']">
              <xsl:apply-templates select="/reflection/apis/api[apidata/@group='namespace']"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="/reflection/apis/api[apidata/@group='type']"/>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="api">
    <div id="{file/@name}">
      <xsl:attribute name="class">
        <xsl:choose>
          <xsl:when test="count(elements/element) &gt; 0 and count(apidata[@subgroup='enumeration']) = 0">
            <xsl:value-of select="string('folder')" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="string('document')" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <span class="label" onclick="ToggleSubItems(this, this.parentNode.getAttribute('id'));">
        <a href="{file/@name}.htm" target="contentFrame" title="{document(concat($html,'/', file/@name, '.htm'),.)/html/head/title}">
          <xsl:value-of select="document(concat($html,'/', file/@name, '.htm'),.)/html/head/title"/>
        </a>
      </span>
      <!-- <xsl:value-of select="apidata/@name"/> -->
      <xsl:if test="count(elements/element) &gt; 0 and count(apidata[@subgroup='enumeration']) = 0">
        <span class="subItems" id="{file/@name}SubItems" style="display:none;">
          <xsl:choose>
            <xsl:when test="apidata/@group='type'">
              <xsl:call-template name="typeElements" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:for-each select="elements/element">
                <xsl:sort select="@api" />
                <xsl:apply-templates select="key('index',@api)" />
              </xsl:for-each>
              <!--
					    <xsl:apply-templates select="key('index',elements/element/@api)">
						    <xsl:sort select="apidata/@name" />
					    </xsl:apply-templates>
              -->
            </xsl:otherwise>
          </xsl:choose>
        </span>
      </xsl:if>
    </div>
  </xsl:template>

  <xsl:template name="typeElements">
    <xsl:variable name="typeId" select="@id" />
    <xsl:variable name="members" select="key('index',elements/element/@api)[containers/type/@api=$typeId]" />
    <xsl:for-each select="$members">
      <xsl:sort select="apidata/@name" />
      <xsl:variable name="name" select="apidata/@name" />
      <xsl:variable name="subgroup" select="apidata/@subgroup" />
      <xsl:variable name="set" select="$members[apidata/@name=$name and apidata/@subgroup=$subgroup]" />
      <xsl:choose>
        <xsl:when test="count($set) &gt; 1">
          <xsl:if test="($set[1]/@id)=@id">
            <xsl:variable name="overloadId">
              <xsl:call-template name="overloadId">
                <xsl:with-param name="memberId" select="@id" />
              </xsl:call-template>
            </xsl:variable>
            <div class="document" id="{@id}">
              <span class="label" onclick="ToggleSubItems(this, this.parentNode.getAttribute('id'));">
                <a href="{key('index',$overloadId)/file/@name}.htm" target="contentFrame"
                   title="{document(concat($html,'/', key('index',$overloadId)/file/@name, '.htm'),.)/html/head/title}">
                  <xsl:value-of select="document(concat($html,'/', key('index',$overloadId)/file/@name, '.htm'),.)/html/head/title"/>
                </a>
              </span>
              <span class="subItems" id="{@id}SubItems" style="display:none;">
                <xsl:for-each select="$set">
                  <xsl:apply-templates select="." />
                </xsl:for-each>
              </span>
            </div>
          </xsl:if>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="." />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
	</xsl:template>

	<xsl:template name="overloadId">
		<xsl:param name="memberId" />
		<xsl:text>Overload:</xsl:text>
		<xsl:variable name="noParameters">
			<xsl:choose>
				<xsl:when test="contains($memberId,'(')">
					<xsl:value-of select="substring-before($memberId,'(')" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$memberId" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="noGeneric">
			<xsl:choose>
				<xsl:when test="contains($noParameters,'``')">
					<xsl:value-of select="substring-before($noParameters,'``')" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$noParameters" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:value-of select="substring($noGeneric,3)" />
	</xsl:template>

</xsl:stylesheet>
